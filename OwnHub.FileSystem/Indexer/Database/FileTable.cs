﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using OwnHub.Database.Table;
using IDbTransaction = OwnHub.Database.IDbTransaction;

namespace OwnHub.FileSystem.Indexer.Database
{
    /// <summary>
    /// The database table for <seealso cref="DatabaseFileIndexer"/>.
    /// </summary>
    internal class FileTable : Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTable"/> class.
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        public FileTable(string tableName)
            : base(tableName)
        {
        }

        /// <inheritdoc/>
        protected override string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Path TEXT NOT NULL UNIQUE,
                Parent INTEGER,
                IsDirectory BOOL NOT NULL,
                IdentifierTag TEXT,
                ContentTag TEXT
            );

            CREATE TABLE IF NOT EXISTS {TableName}Metadata (
                Id INTEGER PRIMARY KEY,
                Target INTEGER NOT NULL REFERENCES {TableName}(Id) ON DELETE CASCADE,
                Key TEXT NOT NULL,
                Data TEXT
            );
            CREATE UNIQUE INDEX IF NOT EXISTS {TableName}MetadataTargetKeyUniqueIndex ON {TableName}Metadata (Target, Key);
            ";

        /// <inheritdoc/>
        protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            DROP TABLE IF EXISTS {TableName}Metadata;
            ";

        private string InsertCommand => $@"
            INSERT INTO {TableName} (Path, Parent, IsDirectory, IdentifierTag, ContentTag) VALUES(
                ?1, ?2, ?3, ?4, ?5
            );
            SELECT last_insert_rowid();
            ";

        private string InsertMetadataCommand => $@"
            INSERT INTO {TableName}Metadata (Target, Key, Data) VALUES(
                ?1, ?2, ?3
            );
            SELECT last_insert_rowid();
            ";

        private string InsertOrReplaceMetadataCommand => $@"
            INSERT OR REPLACE INTO {TableName}Metadata (Target, Key, Data) VALUES(
                ?1, ?2, ?3
            );
            SELECT last_insert_rowid();
            ";

        private string SelectByPathCommand => $@"
            SELECT Id, Path, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Path=?1;
            ";

        private string SelectByParentCommand => $@"
            SELECT Id, Path, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Parent=?1;
            ";

        private string SelectMetadataByTargetCommand => $@"
            SELECT Id, Target, Key, Data FROM {TableName}Metadata
                    WHERE Target = ?1;
            ";

        private string SelectByStartsWithPathCommand => $@"
            SELECT Id, Path, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Path LIKE ?1;
            ";

        private string SelectMetadataByStartsWithPathCommand => $@"
            SELECT {TableName}Metadata.Id, {TableName}Metadata.Target, {TableName}Metadata.Key, {TableName}Metadata.Data
                    FROM {TableName}Metadata JOIN {TableName} ON {TableName}Metadata.Target={TableName}.Id
                    WHERE {TableName}.Path LIKE ?1;
            ";

        private string DeleteByStartsWithPathCommand => $@"
            DELETE FROM {TableName} WHERE Path LIKE ?1;
            ";

        private string UpdateContentTagByIdCommand => $@"
            UPDATE {TableName} SET ContentTag = ?2 WHERE Id = ?1;
            ";

        private string UpdateIdentifierAndContentTagByIdCommand => $@"
            UPDATE {TableName} SET IdentifierTag = ?2, ContentTag = ?3 WHERE Id = ?1;
            ";

        public async Task<long> InsertAsync(
            IDbTransaction transaction,
            string path,
            long? parent,
            bool isDirectory,
            string? identifierTag,
            string? contentTag)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertCommand,
                $"{nameof(FileTable)}/{nameof(InsertCommand)}/{TableName}",
                path,
                parent,
                isDirectory,
                identifierTag,
                contentTag))!;
        }

        public async Task<long> InsertMetadataAsync(
            IDbTransaction transaction,
            long target,
            string key,
            string? data = null)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertMetadataCommand,
                $"{nameof(FileTable)}/{nameof(InsertMetadataCommand)}/{TableName}",
                target,
                key,
                data))!;
        }

        public async Task<long> InsertOrReplaceMetadataAsync(
            IDbTransaction transaction,
            long target,
            string key,
            string? data = null)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertOrReplaceMetadataCommand,
                $"{nameof(FileTable)}/{nameof(InsertOrReplaceMetadataCommand)}/{TableName}",
                target,
                key,
                data))!;
        }

        public Task<DataRow?> SelectByPathAsync(
            IDbTransaction transaction,
            string path)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectByPathCommand,
                $"{nameof(FileTable)}/{nameof(SelectByPathCommand)}/{TableName}",
                HandleReaderSingleDataRow,
                path);
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

        public Task<MetadataDataRow[]> SelectMetadataByTargetAsync(
            IDbTransaction transaction,
            long target)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectMetadataByTargetCommand,
                $"{nameof(FileTable)}/{nameof(SelectMetadataByTargetCommand)}/{TableName}",
                HandleReaderMetadataDataRows,
                target);
        }

        public Task<DataRow[]> SelectByStartsWithAsync(
            IDbTransaction transaction,
            string startsWithPath)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectByStartsWithPathCommand,
                $"{nameof(FileTable)}/{nameof(SelectByStartsWithPathCommand)}/{TableName}",
                HandleReaderDataRows,
                startsWithPath + "%");
        }

        public Task<MetadataDataRow[]> SelectMetadataByStartsWithAsync(
            IDbTransaction transaction,
            string startsWithPath)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectMetadataByStartsWithPathCommand,
                $"{nameof(FileTable)}/{nameof(SelectMetadataByStartsWithPathCommand)}/{TableName}",
                HandleReaderMetadataDataRows,
                startsWithPath + "%");
        }

        /// <summary>
        /// TODO: use sqlite returning statement.
        /// https://github.com/ericsink/SQLitePCL.raw/issues/416
        /// </summary>
        public Task DeleteByStartsWithAsync(IDbTransaction transaction, string startsWithPath)
        {
            return transaction.ExecuteNonQueryAsync(
                () => DeleteByStartsWithPathCommand,
                $"{nameof(FileTable)}/{nameof(DeleteByStartsWithPathCommand)}/{TableName}",
                startsWithPath + "%");
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

        private MetadataDataRow[] HandleReaderMetadataDataRows(IDataReader reader)
        {
            var result = new List<MetadataDataRow>();

            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                result.Add(
                    new MetadataDataRow(
                        reader.GetInt64(0),
                        reader.GetInt64(1),
                        reader.GetString(2),
                        reader.GetValue(3) as string));
            }

            return result.ToArray();
        }

        public record DataRow(long Id, string Path, long? Parent, bool IsDirectory, string? IdentifierTag, string? ContentTag);

        public record MetadataDataRow(long Id, long Target, string Key, string? Data);
    }
}
