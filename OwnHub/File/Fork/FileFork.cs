using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Sqlite;

namespace OwnHub.File.Fork
{
    public class FileForkDatabase : IDisposable
    {
        private const string DatabaseAfterConnectCommand = @"
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            ";

        private const string DatabaseDropCommand = @"
            DROP TABLE IF EXISTS Data;
            DROP TABLE IF EXISTS Fork;
            ";

        private const string DatabaseCreateCommand = @"
            CREATE TABLE Fork
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ParentFile TEXT NOT NULL,
                CreationTime DATETIME,
                ModifyTime DATETIME,
                Type TEXT,
                Payload TEXT,
                UniqueKey TEXT UNIQUE NOT NULL
            );
            CREATE TABLE Data
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CreationTime DATETIME,
                Description TEXT,
                ForkId INTEGER,
                Data BLOB,
                CONSTRAINT FK_ForkId
                FOREIGN KEY (ForkId)
                REFERENCES Fork(Id)
                ON DELETE CASCADE
            );
            CREATE INDEX ForkParentFileIndex ON Fork (ParentFile);
            CREATE INDEX DataForkIdIndex ON Data (ForkId);
            ";

        private const int DatabaseVersion = 2;

        private SqliteContext sqliteContext;
        protected bool Disposed;

        public FileForkDatabase(string databaseFile)
        {
            sqliteContext = new SqliteContext(databaseFile);
        }

        public async Task Open()
        {
            await sqliteContext.Create(async (connection) =>
            {
                SqliteCommand afterConnectCommand = connection.CreateCommand();
                afterConnectCommand.CommandText = DatabaseAfterConnectCommand;
                await afterConnectCommand.ExecuteNonQueryAsync();

                SqliteCommand versionCommand = connection.CreateCommand();
                versionCommand.CommandText =
                    @"PRAGMA user_version;";
                var version = (int) (long) await versionCommand.ExecuteScalarAsync();
                if (version == DatabaseVersion) return;

                SqliteCommand dropCommand = connection.CreateCommand();
                dropCommand.CommandText = DatabaseDropCommand;
                await dropCommand.ExecuteNonQueryAsync();

                SqliteCommand createCommand = connection.CreateCommand();
                createCommand.CommandText = DatabaseCreateCommand;
                await createCommand.ExecuteNonQueryAsync();

                SqliteCommand updateVersionCommand = connection.CreateCommand();
                updateVersionCommand.CommandText =
                    @$"PRAGMA user_version = {DatabaseVersion};";
                await updateVersionCommand.ExecuteNonQueryAsync();
            });
        }

        public Task<T> Add<T>(T fork, string? UniqueKey = null) where T : FileFork
        {
            if (UniqueKey == null)
            {
                UniqueKey = Utils.FunctionUtils.RandomString(32);
            }

            DateTimeOffset creationTime = DateTimeOffset.Now;
            DateTimeOffset modifyTime = DateTimeOffset.Now;

            return sqliteContext.Write(async (connection) =>
            {
                await using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                INSERT OR REPLACE INTO Fork (ParentFile, CreationTime, ModifyTime, Type, Payload, UniqueKey)
                VALUES (
                    $ParentFile,
                    $CreationTime,
                    $ModifyTime,
                    $Type,
                    $Payload,
                    $UniqueKey
                );
                SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("$ParentFile", fork.ParentFile);
                command.Parameters.AddWithValue("$CreationTime", creationTime);
                command.Parameters.AddWithValue("$ModifyTime", modifyTime);
                command.Parameters.AddWithValue("$Type", GetTypeName(fork.GetType()));
                command.Parameters.AddWithValue("$Payload", fork.SerializePayload());
                command.Parameters.AddWithValue("UniqueKey", UniqueKey);

                var id = (long) await command.ExecuteScalarAsync();

                fork.Id = id;
                fork.CreationTime = creationTime;
                fork.ModifyTime = modifyTime;
                fork.Database = this;
                return fork;
            });
        }

        public async Task<T?> GetFork<T>(string parentFile) where T : FileFork
        {
            T[] forks = await GetForks<T>(parentFile, 1);
            return forks.Length == 0 ? null : forks[0];
        }

        public Task<T[]> GetForks<T>(string parentFile) where T : FileFork
        {
            return GetForks<T>(parentFile, null);
        }

        private Task<T[]> GetForks<T>(string parentFile, int? limitSize) where T : FileFork
        {
            return sqliteContext.Read(async (connection) =>
            {
                await using SqliteCommand selectCommand = connection.CreateCommand();
                selectCommand.CommandText = @"
                SELECT Id, CreationTime, ModifyTime, Payload
                FROM Fork WHERE Type = $Type AND ParentFile = $ParentFile
                ORDER BY CreationTime DESC;
                ";
                selectCommand.Parameters.AddWithValue("$Type", GetTypeName(typeof(T)));
                selectCommand.Parameters.AddWithValue("$ParentFile", parentFile);

                await using SqliteDataReader reader = await selectCommand.ExecuteReaderAsync();

                List<T> result = new List<T>();

                while (reader.Read() && (limitSize == null || result.Count < limitSize))
                {
                    long id = reader.GetInt64(0);
                    DateTimeOffset? creationTime =
                        !reader.IsDBNull(1) ? reader.GetDateTimeOffset(1) : (DateTimeOffset?) null;
                    DateTimeOffset? modifyTime =
                        !reader.IsDBNull(2) ? reader.GetDateTimeOffset(2) : (DateTimeOffset?) null;
                    string? payloadJson = !reader.IsDBNull(3) ? reader.GetString(3) : null;

                    ConstructorInfo? constructor = typeof(T).GetConstructor(new[] {typeof(string)});

                    if (constructor != null)
                    {
                        var fork = (T) constructor.Invoke(new object?[] {parentFile});
                        fork.Id = id;
                        fork.CreationTime = creationTime;
                        fork.ModifyTime = modifyTime;
                        if (!string.IsNullOrEmpty(payloadJson)) fork.DeserializePayload(payloadJson);
                        fork.Database = this;

                        FileFork.Data[] dataList = await GetData(fork);
                        fork.AddData(dataList);

                        result.Add(fork);
                    }
                    else
                    {
                        throw new ArgumentException("The fork type does not have a constructor with a single string.");
                    }
                }

                return result.ToArray();
            });
        }

        public Task SaveChanges(FileFork fork)
        {
            return sqliteContext.Write(async (connection) =>
            {
                if (fork.Id == null)
                    throw new InvalidOperationException("The fork should be added before save changes.");
                DateTimeOffset modifyTime = DateTimeOffset.Now;
                await using SqliteCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = @"
                UPDATE Fork SET
                ModifyTime = $ModifyTime,
                Payload = $Payload
                WHERE Id = $Id;
                ";
                updateCommand.Parameters.AddWithValue("$ModifyTime", modifyTime);
                updateCommand.Parameters.AddWithValue("$Payload", fork.SerializePayload());
                updateCommand.Parameters.AddWithValue("$Id", fork.Id);
                await updateCommand.ExecuteNonQueryAsync();

                fork.ModifyTime = modifyTime;
            });
        }

        public Task DeleteFork(FileFork fork)
        {
            return sqliteContext.Write(async (connection) =>
            {
                await using SqliteCommand deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText =
                    @"delete from Fork where Id = $Id;";
                deleteCommand.Parameters.AddWithValue("$Id", fork.Id);
                await deleteCommand.ExecuteNonQueryAsync();
            });
        }

        public Task<FileFork.Data[]> GetData(FileFork fork)
        {
            return sqliteContext.Read(async (connection) =>
            {
                await using SqliteCommand selectCommand = connection.CreateCommand();
                selectCommand.CommandText = @"
                SELECT Id, CreationTime, Description, length(Data)
                FROM Data WHERE ForkId = $Id;
                ";
                selectCommand.Parameters.AddWithValue("$Id", fork.Id);

                await using SqliteDataReader reader = await selectCommand.ExecuteReaderAsync();

                List<FileFork.Data> result = new List<FileFork.Data>();
                while (reader.Read())
                {
                    long id = reader.GetInt64(0);
                    DateTimeOffset creationTime = reader.GetDateTimeOffset(1);
                    string? description = !reader.IsDBNull(2) ? reader.GetString(2) : null;
                    int size = reader.GetInt32(3);

                    var data = new FileFork.Data(this, id, fork, creationTime, size, description);
                    result.Add(data);
                }

                return result.ToArray();
            });
        }

        public Task<FileFork.Data> AddData(FileFork fork, Stream stream, string? description = null)
        {
            if (fork.Id == null) throw new InvalidOperationException("The fork should be added before create data.");
            DateTimeOffset creationTime = DateTimeOffset.Now;
            DateTimeOffset modifyTime = DateTimeOffset.Now;
            long size = stream.Length - stream.Position;

            return sqliteContext.Write(async (connection) =>
            {
                await using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Data (CreationTime, Description, ForkId, Data)
                VALUES (
                    $CreationTime,
                    $Description,
                    $ForkId,
                    zeroblob($Length)
                );
                SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("$CreationTime", creationTime);
                command.Parameters.AddWithValue("$Description", description);
                command.Parameters.AddWithValue("$ForkId", fork.Id);
                command.Parameters.AddWithValue("$Length", size);

                var id = (long) await command.ExecuteScalarAsync();

                await using (SqliteBlob writeStream = sqliteContext.OpenWriteBlob("Data", "Data", id))
                {
                    await stream.CopyToAsync(writeStream);
                }

                var data = new FileFork.Data(this, id, fork, creationTime, size, description);
                return data;
            });
            
        }

        public async Task<Stream> ReadData(FileFork.Data data)
        {
            return await Task.FromResult(sqliteContext.OpenReadBlob("Data", "Data", data.Id));
        }

        public Task DeleteData(FileFork.Data data)
        {
            return sqliteContext.Write(async (connection) =>
            {
                await using SqliteCommand deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText =
                    @"delete from Data where Id = $Id;";
                deleteCommand.Parameters.AddWithValue("$Id", data.Id);
                await deleteCommand.ExecuteNonQueryAsync();
            });

        }

        public Task DeleteAllData(FileFork fork)
        {
            return sqliteContext.Write(async (connection) =>
            {
                await using SqliteCommand deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText =
                    @"delete from Data where ForkId = $ForkId;";
                deleteCommand.Parameters.AddWithValue("$ForkId", fork.Id);
                await deleteCommand.ExecuteNonQueryAsync();
            });
        }

        private static string GetTypeName(Type type)
        {
            return type.Name;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                    sqliteContext = null!;

                Disposed = true;
            }
        }

        ~FileForkDatabase()
        {
            Dispose(false);
        }
    }

    public abstract class FileFork
    {
        public FileForkDatabase? Database { get; set; }
        public long? Id { get; set; }
        public string ParentFile { get; }
        public DateTimeOffset? CreationTime { get; set; }
        public DateTimeOffset? ModifyTime { get; set; }
        private List<Data> dataList = new List<Data>();
        public IEnumerable<Data> DataList => dataList;


        public virtual string? SerializePayload()
        {
            return null;
        }

        public virtual void DeserializePayload(string payloadJson)
        {
            throw new InvalidOperationException();
        }

        public async Task SaveChanges()
        {
            await Database!.SaveChanges(this);
        }

        public void AddData(Data data)
        {
            dataList.Add(data);
        }

        public void AddData(IEnumerable<Data> data)
        {
            dataList.AddRange(data);
        }

        public async Task<Data> AddData(Stream stream, string? description = null)
        {
            Data data = await Database!.AddData(this, stream, description);
            AddData(data);
            return data;
        }

        public Data? GetData(long id)
        {
            return dataList.FirstOrDefault((data) => data.Id == id);
        }

        public async Task DeleteAllData()
        {
            await Database!.DeleteAllData(this);
            dataList.Clear();
        }

        public async Task Delete()
        {
            await Database!.DeleteFork(this);
        }

        public FileFork(string parentFile)
        {
            ParentFile = parentFile;
        }

        public class Data
        {
            public FileForkDatabase Database { get; }
            public long Id { get; }
            public FileFork Fork { get; }
            public DateTimeOffset CreationTime { get; }
            public string? Description { get; }
            public long Size { get; }

            public Data(FileForkDatabase database, long id, FileFork fork, DateTimeOffset creationTime, long size,
                string? description)
            {
                Database = database;
                Id = id;
                Fork = fork;
                CreationTime = creationTime;
                Description = description;
                Size = size;
            }

            public async Task<Stream> Read()
            {
                return await Database.ReadData(this);
            }

            public async Task Delete()
            {
                await Database!.DeleteData(this);
            }
        }
    }

    public abstract class FileFork<T> : FileFork
    {
        public virtual T Payload { get; set; } = default!;

        public FileFork(string parentFile) : base(parentFile)
        {
        }

        public override string? SerializePayload()
        {
            return JsonSerializer.Serialize<T>(this.Payload, new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            });
        }

        public override void DeserializePayload(string payloadJson)
        {
            Payload = JsonSerializer.Deserialize<T>(payloadJson)!;
        }
    }
}