using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using IDbTransaction = Anything.Database.IDbTransaction;

namespace Anything.FileSystem.Tracker.Database
{
    /// <summary>
    ///     The database table for <seealso cref="DatabaseHintFileTracker" />.
    /// </summary>
    internal class FileTable : Table
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileTable" /> class.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public FileTable(string tableName)
            : base(tableName)
        {
        }

        /// <inheritdoc />
        protected override string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Url TEXT NOT NULL UNIQUE,
                Parent INTEGER,
                IsDirectory BOOL NOT NULL,
                IdentifierTag TEXT,
                ContentTag TEXT
            );

            CREATE TABLE IF NOT EXISTS {TableName}AttachedData (
                Id INTEGER PRIMARY KEY,
                Target INTEGER NOT NULL REFERENCES {TableName}(Id) ON DELETE CASCADE,
                Payload TEXT NOT NULL,
                DeletionPolicy INTEGER NOT NULL
            );
            ";

        /// <inheritdoc />
        protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            DROP TABLE IF EXISTS {TableName}AttachedData;
            ";

        private string InsertCommand => $@"
            INSERT INTO {TableName} (Url, Parent, IsDirectory, IdentifierTag, ContentTag) VALUES(
                ?1, ?2, ?3, ?4, ?5
            );
            SELECT last_insert_rowid();
            ";

        private string InsertOrReplaceAttachedDataCommand => $@"
            INSERT OR REPLACE INTO {TableName}AttachedData (Target, Payload, DeletionPolicy) VALUES(
                ?1, ?2, ?3
            );
            SELECT last_insert_rowid();
            ";

        private string SelectByUrlCommand => $@"
            SELECT Id, Url, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Url=?1;
            ";

        private string SelectByParentCommand => $@"
            SELECT Id, Url, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Parent=?1;
            ";

        private string SelectAttachedDataByTargetCommand => $@"
            SELECT Id, Target, Payload, DeletionPolicy FROM {TableName}AttachedData
                    WHERE Target = ?1;
            ";

        private string SelectByStartsWithUrlCommand => $@"
            SELECT Id, Url, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Url LIKE ?1 ESCAPE '\';
            ";

        private string SelectAttachedDataByStartsWithUrlCommand => $@"
            SELECT {TableName}AttachedData.Id, {TableName}AttachedData.Target, {TableName}AttachedData.Payload, {TableName}AttachedData.DeletionPolicy
                    FROM {TableName}AttachedData JOIN {TableName} ON {TableName}AttachedData.Target={TableName}.Id
                    WHERE {TableName}.Url LIKE ?1 ESCAPE '\';
            ";

        private string DeleteByStartsWithUrlCommand => $@"
            DELETE FROM {TableName} WHERE Url LIKE ?1 ESCAPE '\';
            ";

        private string DeleteByUrlCommand => $@"
            DELETE FROM {TableName} WHERE Url = ?1;
            ";

        private string DeleteAttachedDataByIdCommand => $@"
            DELETE FROM {TableName}AttachedData WHERE Id = ?1;
            ";

        private string UpdateContentTagByIdCommand => $@"
            UPDATE {TableName} SET ContentTag = ?2 WHERE Id = ?1;
            ";

        private string UpdateIdentifierAndContentTagByIdCommand => $@"
            UPDATE {TableName} SET IdentifierTag = ?2, ContentTag = ?3 WHERE Id = ?1;
            ";

        public async ValueTask<long> InsertAsync(
            IDbTransaction transaction,
            string url,
            long? parent,
            bool isDirectory,
            string? identifierTag,
            string? contentTag)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertCommand,
                $"{nameof(FileTable)}/{nameof(InsertCommand)}/{TableName}",
                url,
                parent,
                isDirectory,
                identifierTag,
                contentTag))!;
        }

        public async ValueTask<long> InsertOrReplaceAttachedDataAsync(
            IDbTransaction transaction,
            long target,
            FileAttachedData fileAttachedData)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertOrReplaceAttachedDataCommand,
                $"{nameof(FileTable)}/{nameof(InsertOrReplaceAttachedDataCommand)}/{TableName}",
                target,
                fileAttachedData.Payload,
                (int)fileAttachedData.DeletionPolicy))!;
        }

        public ValueTask<DataRow?> SelectByUrlAsync(
            IDbTransaction transaction,
            string url)
        {
            return transaction.ExecuteEnumerableAsync(
                () => SelectByUrlCommand,
                $"{nameof(FileTable)}/{nameof(SelectByUrlCommand)}/{TableName}",
                HandleReaderDataRow,
                url).FirstOrDefaultAsync();
        }

        public IAsyncEnumerable<DataRow> SelectByParentAsync(
            IDbTransaction transaction,
            long parent)
        {
            return transaction.ExecuteEnumerableAsync(
                () => SelectByParentCommand,
                $"{nameof(FileTable)}/{nameof(SelectByParentCommand)}/{TableName}",
                HandleReaderDataRow,
                parent);
        }

        public Task<AttachedDataDataRow[]> SelectAttachedDataByTargetAsync(
            IDbTransaction transaction,
            long target)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectAttachedDataByTargetCommand,
                $"{nameof(FileTable)}/{nameof(SelectAttachedDataByTargetCommand)}/{TableName}",
                HandleReaderAttachedDataDataRows,
                target);
        }

        public IAsyncEnumerable<DataRow> SelectByStartsWithAsync(
            IDbTransaction transaction,
            string startsWith)
        {
            return transaction.ExecuteEnumerableAsync(
                () => SelectByStartsWithUrlCommand,
                $"{nameof(FileTable)}/{nameof(SelectByStartsWithUrlCommand)}/{TableName}",
                HandleReaderDataRow,
                SqliteTransaction.EscapeLikeContent(startsWith) + "%");
        }

        public Task<AttachedDataDataRow[]> SelectAttachedDataByStartsWithAsync(
            IDbTransaction transaction,
            string startsWith)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectAttachedDataByStartsWithUrlCommand,
                $"{nameof(FileTable)}/{nameof(SelectAttachedDataByStartsWithUrlCommand)}/{TableName}",
                HandleReaderAttachedDataDataRows,
                SqliteTransaction.EscapeLikeContent(startsWith) + "%");
        }

        /// <summary>
        ///     TODO: use sqlite returning statement.
        ///     https://github.com/ericsink/SQLitePCL.raw/issues/416.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="startsWith">The start string to deleted.</param>
        public Task DeleteByStartsWithAsync(IDbTransaction transaction, string startsWith)
        {
            return transaction.ExecuteNonQueryAsync(
                () => DeleteByStartsWithUrlCommand,
                $"{nameof(FileTable)}/{nameof(DeleteByStartsWithUrlCommand)}/{TableName}",
                SqliteTransaction.EscapeLikeContent(startsWith) + "%");
        }

        public Task DeleteByUrlAsync(IDbTransaction transaction, string url)
        {
            return transaction.ExecuteNonQueryAsync(
                () => DeleteByUrlCommand,
                $"{nameof(FileTable)}/{nameof(DeleteByUrlCommand)}/{TableName}",
                url);
        }

        public Task DeleteAttachedDataByIdAsync(IDbTransaction transaction, long id)
        {
            return transaction.ExecuteNonQueryAsync(
                () => DeleteAttachedDataByIdCommand,
                $"{nameof(FileTable)}/{nameof(DeleteAttachedDataByIdCommand)}/{TableName}",
                id);
        }

        public Task UpdateContentTagByIdAsync(IDbTransaction transaction, long id, string contentTag)
        {
            return transaction.ExecuteNonQueryAsync(
                () => UpdateContentTagByIdCommand,
                $"{nameof(FileTable)}/{nameof(UpdateContentTagByIdCommand)}/{TableName}",
                id,
                contentTag);
        }

        public Task UpdateIdentifierAndContentTagByIdAsync(IDbTransaction transaction, long id, string identifierTag, string contentTag)
        {
            return transaction.ExecuteNonQueryAsync(
                () => UpdateIdentifierAndContentTagByIdCommand,
                $"{nameof(FileTable)}/{nameof(UpdateIdentifierAndContentTagByIdCommand)}/{TableName}",
                id,
                identifierTag,
                contentTag);
        }

        private DataRow HandleReaderDataRow(IDataReader reader)
        {
            return new(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetValue(2) as long?,
                reader.GetBoolean(3),
                reader.GetValue(4) as string,
                reader.GetValue(5) as string);
        }

        private AttachedDataDataRow[] HandleReaderAttachedDataDataRows(IDataReader reader)
        {
            var result = new List<AttachedDataDataRow>();

            while (reader.Read())
            {
                result.Add(
                    new AttachedDataDataRow(
                        reader.GetInt64(0),
                        reader.GetInt64(1),
                        new FileAttachedData(reader.GetString(2), (FileAttachedData.DeletionPolicies)reader.GetInt32(3))));
            }

            return result.ToArray();
        }

        public record DataRow(long Id, string Url, long? Parent, bool IsDirectory, string? IdentifierTag, string? ContentTag);

        public record AttachedDataDataRow(long Id, long Target, FileAttachedData FileAttachedData);
    }
}
