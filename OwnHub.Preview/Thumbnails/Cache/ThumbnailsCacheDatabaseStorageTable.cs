using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Database;
using OwnHub.Database.Table;
using IDataReader = System.Data.IDataReader;
using SqliteTransaction = OwnHub.Database.SqliteTransaction;

namespace OwnHub.Preview.Thumbnails.Cache
{
    public class ThumbnailsCacheDatabaseStorageTable : Table
    {
        public ThumbnailsCacheDatabaseStorageTable(string tableName)
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

        private string SelectCommand => $@"SELECT Id, Url, Key, Tag FROM {TableName} WHERE Url=?1 AND Tag=?2;";

        private string DeleteByUrlCommand => $@"DELETE FROM {TableName} WHERE Url=?1;";

        public async ValueTask InsertOrReplaceAsync(IDbTransaction transaction, string url, string key, string tag, byte[] data)
        {
            await transaction.ExecuteNonQueryAsync(
                () => InsertOrReplaceCommand,
                $"{nameof(ThumbnailsCacheDatabaseStorageTable)}/{nameof(InsertOrReplaceCommand)}/{TableName}",
                url,
                key,
                tag,
                data);
        }

        public async ValueTask<DataRow[]> SelectAsync(IDbTransaction transaction, string url, string tag)
        {
            return await transaction.ExecuteReaderAsync(
                () => SelectCommand,
                $"{nameof(ThumbnailsCacheDatabaseStorageTable)}/{nameof(SelectCommand)}/{TableName}",
                HandleReaderDataRows,
                url,
                tag);
        }

        public async ValueTask DeleteByPathAsync(IDbTransaction transaction, string url)
        {
            await transaction.ExecuteNonQueryAsync(
                () => DeleteByUrlCommand,
                $"{nameof(ThumbnailsCacheDatabaseStorageTable)}/{nameof(DeleteByUrlCommand)}/{TableName}",
                url);
        }

        private DataRow[] HandleReaderDataRows(IDataReader reader)
        {
            var result = new List<DataRow>();

            while (reader.Read())
            {
                result.Add(
                    new DataRow(
                        reader.GetInt64(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3)));
            }

            return result.ToArray();
        }

        public byte[] GetData(IDbTransaction transaction, long id)
        {
            if (transaction is SqliteTransaction sqliteTransaction)
            {
                var blob = new SqliteBlob(sqliteTransaction.DbConnection, TableName, "Data", id, true);
                using var memoryStream = new MemoryStream((int)blob.Length);
                blob.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public record DataRow(long Id, string Url, string Key, string Tag);
    }
}
