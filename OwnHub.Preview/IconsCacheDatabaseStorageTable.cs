using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Database;
using OwnHub.Database.Table;

namespace OwnHub.Preview
{
    public class IconsCacheDatabaseStorageTable : Table
    {
        public IconsCacheDatabaseStorageTable(string tableName)
            : base(tableName)
        {
        }

        protected override string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                Id INTEGER PRIMARY KEY,
                Url TEXT NOT NULL,
                Key TEXT NOT NULL,
                Tag TEXT NOT NULL,
                Data BLOB NOT NULL
            );

            CREATE INDEX IF NOT EXISTS {TableName}UrlIndex ON {TableName} (Url);
            CREATE UNIQUE INDEX IF NOT EXISTS {TableName}UrlKeyUniqueIndex ON {TableName} (Url, Key);
            ";

        protected override string DatabaseDropCommand => $@"DROP TABLE IF EXISTS {TableName};";

        private string InsertOrReplaceCommand => $@"INSERT OR REPLACE INTO {TableName} (Url, Key, Tag, Data) VALUES(?1, ?2, ?3, ?4);";

        private string SelectCommand => $@"SELECT Id, Data FROM {TableName} WHERE Url=?1 AND Key=?2 AND Tag=?3;";

        private string DeleteByUrlCommand => $@"DELETE FROM {TableName} WHERE Url=?1;";

        public async ValueTask InsertOrReplaceAsync(IDbTransaction transaction, string url, string key, string tag, byte[] data)
        {
            await transaction.ExecuteNonQueryAsync(
                () => InsertOrReplaceCommand,
                $"{nameof(IconsCacheDatabaseStorageTable)}/{nameof(InsertOrReplaceCommand)}/{TableName}",
                url,
                key,
                tag,
                data);
        }

        public async ValueTask<byte[]?> SelectAsync(IDbTransaction transaction, string url, string key, string tag)
        {
            return await transaction.ExecuteReaderAsync(
                () => SelectCommand,
                $"{nameof(IconsCacheDatabaseStorageTable)}/{nameof(SelectCommand)}/{TableName}",
                (reader) =>
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    if (reader is SqliteDataReader sqliteDataReader)
                    {
                        var stream = sqliteDataReader.GetStream(1);
                        using var memoryStream = new MemoryStream((int)stream.Length);
                        stream.CopyTo(memoryStream);
                        var data = memoryStream.ToArray();
                        return data;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                },
                url,
                key,
                tag);
        }

        public async ValueTask DeleteByPathAsync(IDbTransaction transaction, string url)
        {
            await transaction.ExecuteNonQueryAsync(
                () => DeleteByUrlCommand,
                $"{nameof(IconsCacheDatabaseStorageTable)}/{nameof(DeleteByUrlCommand)}/{TableName}",
                url);
        }
    }
}
