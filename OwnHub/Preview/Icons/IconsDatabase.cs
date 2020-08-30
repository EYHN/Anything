using Microsoft.Data.Sqlite;
using MoreLinq;
using Svg.Picture;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OwnHub.Preview.Icons
{

    public class IconsDatabase : IDisposable
    {
        private bool disposed = false;
        public SqliteConnection Connection;

        public static string SizeToColumeName(int Size)
        {
            return "x" + Size;
        }

        private static readonly string DatabasePostConnectCommand = @"
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
                {IconsConstants.AvailableSize.Select((Size) => SizeToColumeName(Size) + " INTEGER").ToDelimitedString(",")}
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
        private static readonly Int32 DatabaseVersion = BitConverter.ToInt32(
            SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(DatabaseInitiateCommand)),
            0
            );

        public IconsDatabase(string databaseFile, SqliteOpenMode mode = SqliteOpenMode.ReadWriteCreate)
        {
            string connectionString = new SqliteConnectionStringBuilder()
            {
                Mode = mode,
                DataSource = databaseFile
            }.ToString();
            Connection = new SqliteConnection(connectionString);
        }

        public static string CalcFileEtag(DateTimeOffset ModifyTime, long Size)
        {
            byte[] data = Encoding.UTF8.GetBytes(ModifyTime.ToUnixTimeMilliseconds().ToString() + Size.ToString());
            byte[] hash = SHA256.Create().ComputeHash(data);

            string hex = BitConverter.ToString(hash).Replace("-", "");
            return hex;
        }

        public async Task<IconsDatabase> Open()
        {
            await Connection.OpenAsync();

            var postConnectCommand = Connection.CreateCommand();
            postConnectCommand.CommandText = DatabasePostConnectCommand;
            await postConnectCommand.ExecuteNonQueryAsync();

            var versionCommand = Connection.CreateCommand();
            versionCommand.CommandText =
            @"PRAGMA user_version;";
            int version = (int)(long)await versionCommand.ExecuteScalarAsync();
            if (version == DatabaseVersion) return this;

            Console.WriteLine("Initiate Database.");
            var initiateCommand = Connection.CreateCommand();
            initiateCommand.CommandText = DatabaseInitiateCommand;
            await initiateCommand.ExecuteNonQueryAsync();

            var updateVersionCommand = Connection.CreateCommand();
            updateVersionCommand.CommandText =
            @$"PRAGMA user_version = {DatabaseVersion};";
            await updateVersionCommand.ExecuteNonQueryAsync();
            return this;
        }

        public async Task<IconEntity> OpenOrCreateOrUpdate(
            string Identifier,
            string Etag
            )
        {
            var old = await Read(Identifier);

            if (old != null)
            {
                if (old.Etag == Etag) return old;
                return await old.Update(Etag);
            } else
            {
                return await Create(Identifier, Etag);
            }
        }

        public async Task<IconEntity> Create(string Identifier, string Etag)
        {
            return await IconEntity.Create(this, Identifier, Etag);
        }

        public async Task<IconEntity> Read(string Identifier)
        {
            return await IconEntity.ReadFromIdentifier(this, Identifier);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Connection.Dispose();
                }

                disposed = true;
            }
        }

        ~IconsDatabase()
        {
            Dispose(false);
        }

        public class DataEntity
        {
            public async static Task<long> SaveData(IconsDatabase Database, Stream stream, string Description)
            {
                var CreationTime = DateTimeOffset.Now;
                long Id;
                using (var command = Database.Connection.CreateCommand())
                {
                    command.CommandText =
                    @$"
                    insert into DataTable (CreationTime, Data, Description)
                    values ($CreationTime, zeroblob($Length), $Description);
                    SELECT last_insert_rowid();
                    ";
                    command.Parameters.AddWithValue("$Length", stream.Length);
                    command.Parameters.AddWithValue("$CreationTime", CreationTime);
                    command.Parameters.AddWithValue("$Description", Description);
                    Id = (long)await command.ExecuteScalarAsync();
                }

                using (var writeStream = new SqliteBlob(Database.Connection, "DataTable", "Data", Id))
                {
                    await stream.CopyToAsync(writeStream);
                }

                return Id;
            }

            public static Stream ReadData(IconsDatabase Database, long Id)
            {
                return new SqliteBlob(Database.Connection, "DataTable", "Data", Id, readOnly: true); ;
            }
        }

        public class IconEntity
        {
            public string Identifier { get; set; }
            public string Etag { get; set; }
            public string Source { get; set; }
            public DateTimeOffset CreationTime { get; set; }
            public DateTimeOffset ModifyTime { get; set; }

            public Dictionary<int, long?> DataIds = new Dictionary<int, long?>();

            public readonly long Id;
            public readonly IconsDatabase Database;

            public IconEntity(IconsDatabase Database, long Id)
            {
                this.Database = Database;
                this.Id = Id;

                IconsConstants.AvailableSize.ForEach((Size) => DataIds[Size] = null);
            }

            public static async Task<IconEntity> Create(IconsDatabase Database, string Identifier, string Etag)
            {
                var CreationTime = DateTimeOffset.Now;
                var ModifyTime = DateTimeOffset.Now;

                var command = Database.Connection.CreateCommand();
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
                command.Parameters.AddWithValue("$Identifier", Identifier);
                command.Parameters.AddWithValue("$Etag", Etag);
                command.Parameters.AddWithValue("$CreationTime", CreationTime);
                command.Parameters.AddWithValue("$ModifyTime", ModifyTime);
                long iconId = (long)await command.ExecuteScalarAsync();

                return new IconEntity(Database, iconId)
                {
                    Identifier = Identifier,
                    Etag = Etag,
                    CreationTime = CreationTime,
                    ModifyTime = ModifyTime
                };
            }

            public async static Task<IconEntity> ReadFromIdentifier(IconsDatabase Database, string Identifier)
            {
                var selectCommand = Database.Connection.CreateCommand();
                selectCommand.CommandText =
                @$"
                select Etag, CreationTime, ModifyTime, Id , {IconsConstants.AvailableSize.Select((Size) => SizeToColumeName(Size)).ToDelimitedString(",")}
                from IconsTable where Identifier = $Identifier;
                ";
                selectCommand.Parameters.AddWithValue("$Identifier", Identifier);

                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        string Etag = reader.GetString(0);
                        DateTimeOffset CreationTime = reader.GetDateTimeOffset(1);
                        DateTimeOffset ModifyTime = reader.GetDateTimeOffset(2);
                        long Id = reader.GetInt64(3);
                        Dictionary<int, long?> DataIds = new Dictionary<int, long?>();

                        IconsConstants.AvailableSize.ForEach((Size, Index) =>
                        {
                            DataIds[Size] = reader.GetInt64OrNull(4 + Index);
                        });

                        return new IconEntity(Database, Id)
                        {
                            Identifier = Identifier,
                            Etag = Etag,
                            CreationTime = CreationTime,
                            ModifyTime = ModifyTime,
                            DataIds = DataIds
                        };
                    }
                    return null;
                }
            }

            public async Task<IconEntity> Update(string Etag)
            {
                var ModifyTime = DateTimeOffset.Now;
                var command = Database.Connection.CreateCommand();
                command.CommandText =
                @$"
                BEGIN;
                delete from DataTable where Id in (
                    {IconsConstants.AvailableSize.Select((Size) => $"SELECT {SizeToColumeName(Size)} from IconsTable where Id = $Id").ToDelimitedString(" UNION ALL ")}
                );
                update IconsTable set
                    Etag = $Etag,
                    ModifyTime = $ModifyTime,
                    {IconsConstants.AvailableSize.Select((Size) => $"{SizeToColumeName(Size)} = NULL").ToDelimitedString(",")}
                    where Id = $Id;
                COMMIT;
                ";
                command.Parameters.AddWithValue("$Etag", Etag);
                command.Parameters.AddWithValue("$ModifyTime", ModifyTime);
                command.Parameters.AddWithValue("$Id", Id);
                await command.ExecuteNonQueryAsync();

                this.Etag = Etag;
                this.ModifyTime = ModifyTime;
                var DataIds = new Dictionary<int, long?>();
                IconsConstants.AvailableSize.ForEach((Size) => DataIds[Size] = null);

                return this;
            }

            public async Task<IconEntity> Write(int Size, Stream stream)
            {
                var DataId = await DataEntity.SaveData(Database, stream, Identifier + " - " + Size);

                var ModifyTime = DateTimeOffset.Now;
                using (var command = Database.Connection.CreateCommand())
                {
                    command.CommandText =
                    @$"
                    BEGIN;
                    delete from DataTable where Id in (
                        SELECT {SizeToColumeName(Size)} from IconsTable where Id = $Id
                    );
                    update IconsTable set {SizeToColumeName(Size)} = $DataId, ModifyTime = $ModifyTime where Id = $Id;
                    COMMIT;
                    ";
                    command.Parameters.AddWithValue("$DataId", DataId);
                    command.Parameters.AddWithValue("$Id", Id);
                    command.Parameters.AddWithValue("$ModifyTime", ModifyTime);
                    await command.ExecuteNonQueryAsync();
                }

                this.ModifyTime = ModifyTime;
                return this;
            }

            public Stream Read(int Size)
            {
                long? dataId = DataIds[Size];

                if (dataId != null)
                {
                    return DataEntity.ReadData(Database, (long)dataId);
                }
                return null;
            }

            public Stream GetStream(int Size)
            {
                long? dataId = DataIds[Size];

                if (dataId != null)
                {
                    return DataEntity.ReadData(Database, (long)dataId);
                }
                return null;
            }

            public bool Has(int Size) => DataIds[Size] != null;
        }
    }

    static class DbDataReaderExtensions
    {
        public static long? GetInt64OrNull(this DbDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (long?)reader.GetInt64(ordinal);
        }
    }
}
