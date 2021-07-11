using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Anything.Database.Table;
using Microsoft.Data.Sqlite;
using IDbTransaction = Anything.Database.IDbTransaction;
using SqliteTransaction = Anything.Database.SqliteTransaction;

namespace Anything.Preview.Thumbnails.Cache
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
            ";

        protected override string DatabaseDropCommand => $@"DROP TABLE IF EXISTS {TableName};";

        private string InsertOrReplaceCommand => $@"
            INSERT OR REPLACE INTO {TableName} (Url, Key, Tag, Data) VALUES(?1, ?2, ?3, ?4);
            SELECT last_insert_rowid();
            ";

        private string SelectCommand => $@"SELECT Id, Url, Key, Tag FROM {TableName} WHERE Url=?1 AND Tag=?2;";

        private string DeleteCommand => $@"DELETE FROM {TableName} WHERE Id = ?1;";

        private string CountCommand => $@"SELECT COUNT(*) FROM {TableName};";

        public async ValueTask<long> InsertOrReplaceAsync(IDbTransaction transaction, string url, string key, string tag, byte[] data)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertOrReplaceCommand,
                $"{nameof(ThumbnailsCacheDatabaseStorageTable)}/{nameof(InsertOrReplaceCommand)}/{TableName}",
                url,
                key,
                tag,
                data))!;
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

        public async ValueTask DeleteAsync(IDbTransaction transaction, long id)
        {
            await transaction.ExecuteNonQueryAsync(
                () => DeleteCommand,
                $"{nameof(ThumbnailsCacheDatabaseStorageTable)}/{nameof(DeleteCommand)}/{TableName}",
                id);
        }

        public async ValueTask<long> GetCount(IDbTransaction transaction)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => CountCommand,
                $"{nameof(ThumbnailsCacheDatabaseStorageTable)}/{nameof(CountCommand)}/{TableName}"))!;
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

            throw new NotSupportedException();
        }

        public record DataRow(long Id, string Url, string Key, string Tag);
    }
}
