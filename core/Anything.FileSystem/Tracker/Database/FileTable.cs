using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Anything.Database.Table;
using IDbTransaction = Anything.Database.IDbTransaction;

namespace Anything.FileSystem.Tracker.Database
{
    /// <summary>
    ///     The database table for <seealso cref="DatabaseFileTracker" />.
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

            CREATE TABLE IF NOT EXISTS {TableName}TrackTags (
                Id INTEGER PRIMARY KEY,
                Target INTEGER NOT NULL REFERENCES {TableName}(Id) ON DELETE CASCADE,
                Key TEXT NOT NULL,
                Data TEXT
            );
            CREATE UNIQUE INDEX IF NOT EXISTS {TableName}TrackTagsTargetKeyUniqueIndex ON {TableName}TrackTags (Target, Key);
            ";

        /// <inheritdoc />
        protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            DROP TABLE IF EXISTS {TableName}TrackTags;
            ";

        private string InsertCommand => $@"
            INSERT INTO {TableName} (Url, Parent, IsDirectory, IdentifierTag, ContentTag) VALUES(
                ?1, ?2, ?3, ?4, ?5
            );
            SELECT last_insert_rowid();
            ";

        private string InsertTrackTagCommand => $@"
            INSERT INTO {TableName}TrackTags (Target, Key, Data) VALUES(
                ?1, ?2, ?3
            );
            SELECT last_insert_rowid();
            ";

        private string InsertOrReplaceTrackTagCommand => $@"
            INSERT OR REPLACE INTO {TableName}TrackTags (Target, Key, Data) VALUES(
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

        private string SelectTrackTagsByTargetCommand => $@"
            SELECT Id, Target, Key, Data FROM {TableName}TrackTags
                    WHERE Target = ?1;
            ";

        private string SelectByStartsWithUrlCommand => $@"
            SELECT Id, Url, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Url LIKE ?1;
            ";

        private string SelectTrackTagsByStartsWithUrlCommand => $@"
            SELECT {TableName}TrackTags.Id, {TableName}TrackTags.Target, {TableName}TrackTags.Key, {TableName}TrackTags.Data
                    FROM {TableName}TrackTags JOIN {TableName} ON {TableName}TrackTags.Target={TableName}.Id
                    WHERE {TableName}.Url LIKE ?1;
            ";

        private string DeleteByStartsWithUrlCommand => $@"
            DELETE FROM {TableName} WHERE Url LIKE ?1;
            ";

        private string UpdateContentTagByIdCommand => $@"
            UPDATE {TableName} SET ContentTag = ?2 WHERE Id = ?1;
            ";

        private string UpdateIdentifierAndContentTagByIdCommand => $@"
            UPDATE {TableName} SET IdentifierTag = ?2, ContentTag = ?3 WHERE Id = ?1;
            ";

        public async Task<long> InsertAsync(
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

        public async Task<long> InsertTrackTagAsync(
            IDbTransaction transaction,
            long target,
            string key,
            string? data = null)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertTrackTagCommand,
                $"{nameof(FileTable)}/{nameof(InsertTrackTagCommand)}/{TableName}",
                target,
                key,
                data))!;
        }

        public async Task<long> InsertOrReplaceTrackTagAsync(
            IDbTransaction transaction,
            long target,
            string key,
            string? data = null)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertOrReplaceTrackTagCommand,
                $"{nameof(FileTable)}/{nameof(InsertOrReplaceTrackTagCommand)}/{TableName}",
                target,
                key,
                data))!;
        }

        public Task<DataRow?> SelectByUrlAsync(
            IDbTransaction transaction,
            string url)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectByUrlCommand,
                $"{nameof(FileTable)}/{nameof(SelectByUrlCommand)}/{TableName}",
                HandleReaderSingleDataRow,
                url);
        }

        public Task<DataRow[]> SelectByParentAsync(
            IDbTransaction transaction,
            long parent)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectByParentCommand,
                $"{nameof(FileTable)}/{nameof(SelectByParentCommand)}/{TableName}",
                HandleReaderDataRows,
                parent);
        }

        public Task<TrackTagDataRow[]> SelectTrackTagsByTargetAsync(
            IDbTransaction transaction,
            long target)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectTrackTagsByTargetCommand,
                $"{nameof(FileTable)}/{nameof(SelectTrackTagsByTargetCommand)}/{TableName}",
                HandleReaderTrackTagDataRows,
                target);
        }

        public Task<DataRow[]> SelectByStartsWithAsync(
            IDbTransaction transaction,
            string startsWith)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectByStartsWithUrlCommand,
                $"{nameof(FileTable)}/{nameof(SelectByStartsWithUrlCommand)}/{TableName}",
                HandleReaderDataRows,
                startsWith + "%");
        }

        public Task<TrackTagDataRow[]> SelectTrackTagsByStartsWithAsync(
            IDbTransaction transaction,
            string startsWith)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectTrackTagsByStartsWithUrlCommand,
                $"{nameof(FileTable)}/{nameof(SelectTrackTagsByStartsWithUrlCommand)}/{TableName}",
                HandleReaderTrackTagDataRows,
                startsWith + "%");
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
                startsWith + "%");
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

        private DataRow? HandleReaderSingleDataRow(IDataReader reader)
        {
            if (!reader.Read())
            {
                return null;
            }

            return new DataRow(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetValue(2) as long?,
                reader.GetBoolean(3),
                reader.GetValue(4) as string,
                reader.GetValue(5) as string);
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
                        reader.GetValue(2) as long?,
                        reader.GetBoolean(3),
                        reader.GetValue(4) as string,
                        reader.GetValue(5) as string));
            }

            return result.ToArray();
        }

        private TrackTagDataRow[] HandleReaderTrackTagDataRows(IDataReader reader)
        {
            var result = new List<TrackTagDataRow>();

            while (reader.Read())
            {
                result.Add(
                    new TrackTagDataRow(
                        reader.GetInt64(0),
                        reader.GetInt64(1),
                        reader.GetString(2),
                        reader.GetValue(3) as string));
            }

            return result.ToArray();
        }

        public record DataRow(long Id, string Url, long? Parent, bool IsDirectory, string? IdentifierTag, string? ContentTag);

        public record TrackTagDataRow(long Id, long Target, string Key, string? Data);
    }
}
