using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MoreLinq;
using OwnHub.Utils;

namespace OwnHub.Preview.Icons
{
    public class IconsDatabase : IDisposable
    {
        private static readonly string DatabaseAfterConnectCommand = @"
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            ";

        private static readonly string DatabaseInitiateCommand = @$"
            drop table if exists IconsTable;
            drop table if exists DataTable;
            PRAGMA page_size = 8192;
            create table IconsTable
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Identifier TEXT NOT NULL,
                Etag TEXT NOT NULL,
                CreationTime DATETIME,
                ModifyTime DATETIME,
                {IconsConstants.AvailableSize.Select(size => SizeToColumeName(size) + " INTEGER").ToDelimitedString(",")}
            );
            create table DataTable
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CreationTime DATETIME,
                Description TEXT,
                Data BLOB
            );
            create unique index if not exists IdentifierIndex on IconsTable (Identifier);
            ";

        private static readonly int DatabaseVersion = BitConverter.ToInt32(
            SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(DatabaseInitiateCommand)),
            0
        );

        private readonly object afterConnectLock = new object();
        private readonly List<SqliteConnection> connectionList;

        private readonly ObjectPool<SqliteConnection> connectionPool;
        private readonly string databaseFile;
        private readonly SqliteOpenMode mode;
        private bool afterConnectExecuted;
        private bool disposed;

        public IconsDatabase(string databaseFile, SqliteOpenMode mode = SqliteOpenMode.ReadWriteCreate)
        {
            this.databaseFile = databaseFile;
            this.mode = mode;
            connectionList = new List<SqliteConnection>();
            connectionPool = new ObjectPool<SqliteConnection>(1, () => OpenConnection());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static string SizeToColumeName(int size)
        {
            return "x" + size;
        }

        public static string CalcFileEtag(DateTimeOffset modifyTime, long size)
        {
            byte[] data = Encoding.UTF8.GetBytes(modifyTime.ToUnixTimeMilliseconds() + size.ToString());
            byte[] hash = SHA256.Create().ComputeHash(data);

            string hex = BitConverter.ToString(hash).Replace("-", "");
            return hex;
        }

        private SqliteConnection OpenConnection()
        {
            string connectionString = new SqliteConnectionStringBuilder
            {
                Mode = mode,
                DataSource = databaseFile
            }.ToString();
            SqliteConnection connection = new SqliteConnection(connectionString);
            connectionList.Add(connection);
            connection.Open();

            lock (afterConnectLock)
            {
                if (afterConnectExecuted) return connection;

                try
                {
                    SqliteCommand? postConnectCommand = connection.CreateCommand();
                    postConnectCommand.CommandText = DatabaseAfterConnectCommand;
                    postConnectCommand.ExecuteNonQuery();

                    SqliteCommand? versionCommand = connection.CreateCommand();
                    versionCommand.CommandText =
                        @"PRAGMA user_version;";
                    var version = (int) (long) versionCommand.ExecuteScalar();
                    if (version == DatabaseVersion) return connection;

                    Console.WriteLine("Initiate Database.");
                    SqliteCommand? initiateCommand = connection.CreateCommand();
                    initiateCommand.CommandText = DatabaseInitiateCommand;
                    initiateCommand.ExecuteNonQuery();

                    SqliteCommand? updateVersionCommand = connection.CreateCommand();
                    updateVersionCommand.CommandText =
                        @$"PRAGMA user_version = {DatabaseVersion};";
                    updateVersionCommand.ExecuteNonQuery();
                    return connection;
                }
                finally
                {
                    afterConnectExecuted = true;
                }
            }
        }

        public async Task<IconEntity> OpenOrCreateOrUpdate(
            string identifier,
            string etag
        )
        {
            IconEntity? old = await Read(identifier);

            if (old != null)
            {
                if (old.Etag == etag) return old;
                return await old.Update(etag);
            }

            return await Create(identifier, etag);
        }

        public async Task<IconEntity> Create(string identifier, string etag)
        {
            return await IconEntity.Create(this, identifier, etag);
        }

        public async Task<IconEntity?> Read(string identifier)
        {
            return await IconEntity.ReadFromIdentifier(this, identifier);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    foreach (var connection in connectionList)
                        connection.Dispose();

                disposed = true;
            }
        }

        public async Task<TResult> RunOnDatabaseThread<TResult>(Func<SqliteConnection, TResult> function)
        {
            using (ObjectPool<SqliteConnection>.Container? disposable = await connectionPool.GetContainerAsync())
            {
                SqliteConnection connection = disposable.Value;
                TResult result = function(connection);
                return result;
            }
        }
        
        ~IconsDatabase()
        {
            Dispose(false);
        }

        public class DataEntity
        {
            private static Task<SqliteBlob> OpenBlob(IconsDatabase database, long id, bool readOnly = false)
            {
                return database.RunOnDatabaseThread(connection =>
                    new SqliteBlob(connection, "DataTable", "Data", id, readOnly));
            }

            public static async Task<long> SaveData(IconsDatabase database, Stream stream, string description)
            {
                long id = await database.RunOnDatabaseThread(connection =>
                {
                    using (SqliteCommand? command = connection.CreateCommand())
                    {
                        DateTimeOffset creationTime = DateTimeOffset.Now;
                        long id;
                        command.CommandText = @"
                        insert into DataTable (CreationTime, Data, Description)
                        values ($CreationTime, zeroblob($Length), $Description);
                        SELECT last_insert_rowid();
                        ";
                        command.Parameters.AddWithValue("$Length", stream.Length);
                        command.Parameters.AddWithValue("$CreationTime", creationTime);
                        command.Parameters.AddWithValue("$Description", description);
                        id = (long) command.ExecuteScalar();
                        return id;
                    }
                });

                using (SqliteBlob? writeStream = await OpenBlob(database, id))
                {
                    await stream.CopyToAsync(writeStream);
                }

                return id;
            }

            public static async Task<Stream> ReadData(IconsDatabase database, long id)
            {
                return await OpenBlob(database, id, true);
            }
        }

        public class IconEntity
        {
            public readonly IconsDatabase Database;

            public readonly long Id;

            public Dictionary<int, long?> DataIds = new Dictionary<int, long?>();

            public IconEntity(IconsDatabase database, long id)
            {
                this.Database = database;
                this.Id = id;

                IconsConstants.AvailableSize.ForEach(size => DataIds[size] = null);
            }

            [Required] public string Identifier { get; set; } = null!;

            [Required] public string Etag { get; set; } = null!;

            [Required] public string Source { get; set; } = null!;

            [Required] public DateTimeOffset CreationTime { get; set; }

            [Required] public DateTimeOffset ModifyTime { get; set; }

            public static async Task<IconEntity> Create(IconsDatabase database, string identifier, string etag)
            {
                return await database.RunOnDatabaseThread(connection =>
                {
                    DateTimeOffset creationTime = DateTimeOffset.Now;
                    DateTimeOffset modifyTime = DateTimeOffset.Now;

                    using (SqliteCommand? command = connection.CreateCommand())
                    {
                        command.CommandText =
                            @"
                        insert or replace into IconsTable (Identifier, Etag, CreationTime, ModifyTime)
                        VALUES (
                            $Identifier,
                            $Etag,
                            $CreationTime,
                            $ModifyTime
                        );
                        SELECT last_insert_rowid();
                        ";
                        command.Parameters.AddWithValue("$Identifier", identifier);
                        command.Parameters.AddWithValue("$Etag", etag);
                        command.Parameters.AddWithValue("$CreationTime", creationTime);
                        command.Parameters.AddWithValue("$ModifyTime", modifyTime);
                        var iconId = (long) command.ExecuteScalar();


                        return new IconEntity(database, iconId)
                        {
                            Identifier = identifier,
                            Etag = etag,
                            CreationTime = creationTime,
                            ModifyTime = modifyTime
                        };
                    }
                });
            }

            public static async Task<IconEntity?> ReadFromIdentifier(IconsDatabase database, string identifier)
            {
                return await database.RunOnDatabaseThread(connection =>
                {
                    using (SqliteCommand? selectCommand = connection.CreateCommand())
                    {
                        selectCommand.CommandText =
                            @$"
                        select Etag, CreationTime, ModifyTime, Id , {IconsConstants.AvailableSize.Select(size => SizeToColumeName(size)).ToDelimitedString(",")}
                        from IconsTable where Identifier = $Identifier;
                        ";
                        selectCommand.Parameters.AddWithValue("$Identifier", identifier);

                        using (SqliteDataReader? reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string etag = reader.GetString(0);
                                DateTimeOffset creationTime = reader.GetDateTimeOffset(1);
                                DateTimeOffset modifyTime = reader.GetDateTimeOffset(2);
                                long id = reader.GetInt64(3);
                                Dictionary<int, long?> dataIds = new Dictionary<int, long?>();

                                IconsConstants.AvailableSize.ForEach((size, index) =>
                                {
                                    dataIds[size] = reader.GetInt64OrNull(4 + index);
                                });

                                return new IconEntity(database, id)
                                {
                                    Identifier = identifier,
                                    Etag = etag,
                                    CreationTime = creationTime,
                                    ModifyTime = modifyTime,
                                    DataIds = dataIds
                                };
                            }

                            return null;
                        }
                    }
                });
            }

            public async Task<IconEntity> Update(string etag)
            {
                return await Database.RunOnDatabaseThread(connection =>
                {
                    DateTimeOffset modifyTime = DateTimeOffset.Now;
                    using (SqliteTransaction? transaction = connection.BeginTransaction())
                    {
                        using (SqliteCommand? command = connection.CreateCommand())
                        {
                            command.CommandText =
                                @$"
                            delete from DataTable where Id in (
                                {IconsConstants.AvailableSize.Select(size => $"SELECT {SizeToColumeName(size)} from IconsTable where Id = $Id").ToDelimitedString(" UNION ALL ")}
                            );
                            update IconsTable set
                                Etag = $Etag,
                                ModifyTime = $ModifyTime,
                                {IconsConstants.AvailableSize.Select(size => $"{SizeToColumeName(size)} = NULL").ToDelimitedString(",")}
                                where Id = $Id;
                            ";
                            command.Parameters.AddWithValue("$Etag", etag);
                            command.Parameters.AddWithValue("$ModifyTime", modifyTime);
                            command.Parameters.AddWithValue("$Id", Id);
                            command.ExecuteNonQuery();

                            this.Etag = etag;
                            this.ModifyTime = modifyTime;
                            var dataIds = new Dictionary<int, long?>();
                            IconsConstants.AvailableSize.ForEach(size => dataIds[size] = null);
                        }

                        transaction.Commit();
                    }

                    return this;
                });
            }

            public async Task<IconEntity> Write(int size, Stream stream)
            {
                long dataId = await DataEntity.SaveData(Database, stream, Identifier + " - " + size);

                return await Database.RunOnDatabaseThread(connection =>
                {
                    DateTimeOffset modifyTime = DateTimeOffset.Now;
                    using (SqliteTransaction? transaction = connection.BeginTransaction())
                    {
                        using (SqliteCommand? command = connection.CreateCommand())
                        {
                            command.CommandText =
                                @$"
                            delete from DataTable where Id in (
                                SELECT {SizeToColumeName(size)} from IconsTable where Id = $Id
                            );
                            update IconsTable set {SizeToColumeName(size)} = $DataId, ModifyTime = $ModifyTime where Id = $Id;
                            ";
                            command.Parameters.AddWithValue("$DataId", dataId);
                            command.Parameters.AddWithValue("$Id", Id);
                            command.Parameters.AddWithValue("$ModifyTime", modifyTime);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }

                    this.ModifyTime = modifyTime;
                    return this;
                });
            }

            public async Task<Stream?> Read(int size)
            {
                long? dataId = DataIds[size];

                if (dataId != null) return await DataEntity.ReadData(Database, (long) dataId);
                return null;
            }

            public bool Has(int size)
            {
                return DataIds[size] != null;
            }
        }
    }

    internal static class DbDataReaderExtensions
    {
        public static long? GetInt64OrNull(this DbDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (long?) reader.GetInt64(ordinal);
        }
    }
}