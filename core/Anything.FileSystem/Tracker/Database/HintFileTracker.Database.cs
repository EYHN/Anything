using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Database.Extensions;
using IDbTransaction = Anything.Database.IDbTransaction;

namespace Anything.FileSystem.Tracker.Database
{
    public partial class HintFileTracker
    {
        /// <summary>
        ///     The database table for <seealso cref="HintFileTracker" />.
        /// </summary>
        internal class DatabaseTable : Table
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="DatabaseTable" /> class.
            /// </summary>
            /// <param name="tableName">The name of the table.</param>
            public DatabaseTable(string tableName)
                : base(tableName)
            {
            }

            /// <inheritdoc />
            protected override string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Path TEXT NOT NULL UNIQUE,
                Parent INTEGER,
                IsDirectory INTEGER NOT NULL,
                IdentifierTag TEXT,
                ContentTag TEXT,
                CreationTime INTEGER,
                LastWriteTime INTEGER,
                Size INTEGER,
                Type INTEGER
            );

            CREATE TABLE IF NOT EXISTS {TableName}AttachedData (
                Id INTEGER PRIMARY KEY,
                Target INTEGER NOT NULL REFERENCES {TableName}(Id) ON DELETE CASCADE,
                Payload TEXT NOT NULL
            );
            ";

            /// <inheritdoc />
            protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            DROP TABLE IF EXISTS {TableName}AttachedData;
            ";

            private string InsertCommand => $@"
            INSERT INTO {TableName} (Path, Parent, IsDirectory, IdentifierTag, ContentTag, CreationTime, LastWriteTime, Size, Type) VALUES(
                ?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9
            );
            SELECT last_insert_rowid();
            ";

            private string InsertOrReplaceAttachedDataCommand => $@"
            INSERT OR REPLACE INTO {TableName}AttachedData (Target, Payload) VALUES(
                ?1, ?2
            );
            SELECT last_insert_rowid();
            ";

            private string SelectByPathCommand => $@"
            SELECT Id, Path, Parent, IsDirectory, IdentifierTag, ContentTag, CreationTime, LastWriteTime, Size, Type FROM {TableName}
                    WHERE Path=?1;
            ";

            private string SelectByParentCommand => $@"
            SELECT Id, Path, Parent, IsDirectory, IdentifierTag, ContentTag, CreationTime, LastWriteTime, Size, Type FROM {TableName}
                    WHERE Parent=?1;
            ";

            private string SelectAttachedDataByTargetCommand => $@"
            SELECT Id, Target, Payload FROM {TableName}AttachedData
                    WHERE Target = ?1;
            ";

            private string SelectAttachedDataByTargetAndPrefixCommand => $@"
            SELECT Id, Target, Payload FROM {TableName}AttachedData
                    WHERE Target = ?1 AND Payload LIKE ?2 ESCAPE '\';
            ";

            private string SelectByStartsWithPathCommand => $@"
            SELECT Id, Path, Parent, IsDirectory, IdentifierTag, ContentTag, CreationTime, LastWriteTime, Size, Type FROM {TableName}
                    WHERE Path LIKE ?1 ESCAPE '\';
            ";

            private string SelectAttachedDataByStartsWithPathCommand => $@"
            SELECT {TableName}AttachedData.Id, {TableName}AttachedData.Target, {TableName}AttachedData.Payload
                    FROM {TableName}AttachedData JOIN {TableName} ON {TableName}AttachedData.Target={TableName}.Id
                    WHERE {TableName}.Path LIKE ?1 ESCAPE '\';
            ";

            private string DeleteByStartsWithPathCommand => $@"
            DELETE FROM {TableName} WHERE Path LIKE ?1 ESCAPE '\';
            ";

            private string DeleteByPathCommand => $@"
            DELETE FROM {TableName} WHERE Path = ?1;
            ";

            private string DeleteAttachedDataByIdCommand => $@"
            DELETE FROM {TableName}AttachedData WHERE Id = ?1;
            ";

            private string UpdateStatsByIdCommand => $@"
            UPDATE {TableName} SET ContentTag = ?2, CreationTime = ?3, LastWriteTime = ?4, Size = ?5, Type = ?6 WHERE Id = ?1;
            ";

            private string UpdateIdentifierTagByIdCommand => $@"
            UPDATE {TableName} SET IdentifierTag = ?2, ContentTag = ?3, CreationTime = ?4, LastWriteTime = ?5, Size = ?6, Type = ?7 WHERE Id = ?1;
            ";

            public ValueTask<long> InsertAsync(
                IDbTransaction transaction,
                string path,
                long? parent,
                bool isDirectory,
                FileHandle? fileHandle,
                FileStats? stats)
            {
                return InsertAsync(
                    transaction,
                    path,
                    parent,
                    isDirectory,
                    fileHandle?.Identifier,
                    stats?.Hash.ContentTag,
                    stats?.CreationTime,
                    stats?.LastWriteTime,
                    stats?.Size,
                    stats?.Type);
            }

            public async ValueTask<long> InsertAsync(
                IDbTransaction transaction,
                string path,
                long? parent,
                bool isDirectory,
                string? identifierTag,
                string? contentTag,
                DateTimeOffset? creationTime,
                DateTimeOffset? lastWriteTime,
                long? size,
                FileType? type)
            {
                return (long)(await transaction.ExecuteScalarAsync(
                    () => InsertCommand,
                    $"{nameof(DatabaseTable)}/{nameof(InsertCommand)}/{TableName}",
                    path,
                    parent,
                    isDirectory ? 1 : 0,
                    identifierTag,
                    contentTag,
                    creationTime?.ToUnixTimeMilliseconds(),
                    lastWriteTime?.ToUnixTimeMilliseconds(),
                    size,
                    type))!;
            }

            public async ValueTask<long> InsertOrReplaceAttachedDataAsync(
                IDbTransaction transaction,
                long target,
                FileAttachedData fileAttachedData)
            {
                return (long)(await transaction.ExecuteScalarAsync(
                    () => InsertOrReplaceAttachedDataCommand,
                    $"{nameof(DatabaseTable)}/{nameof(InsertOrReplaceAttachedDataCommand)}/{TableName}",
                    target,
                    fileAttachedData.Payload))!;
            }

            public ValueTask<DataRow?> SelectByPathAsync(
                IDbTransaction transaction,
                string path)
            {
                return transaction.ExecuteReaderAsync(
                    () => SelectByPathCommand,
                    $"{nameof(DatabaseTable)}/{nameof(SelectByPathCommand)}/{TableName}",
                    HandleReaderSingleDataRow,
                    path);
            }

            public ValueTask<DataRow[]> SelectByParentAsync(
                IDbTransaction transaction,
                long parent)
            {
                return transaction.ExecuteReaderAsync(
                    () => SelectByParentCommand,
                    $"{nameof(DatabaseTable)}/{nameof(SelectByParentCommand)}/{TableName}",
                    HandleReaderDataRows,
                    parent);
            }

            public ValueTask<AttachedDataDataRow[]> SelectAttachedDataByTargetAsync(
                IDbTransaction transaction,
                long target)
            {
                return transaction.ExecuteReaderAsync(
                    () => SelectAttachedDataByTargetCommand,
                    $"{nameof(DatabaseTable)}/{nameof(SelectAttachedDataByTargetCommand)}/{TableName}",
                    HandleReaderAttachedDataDataRows,
                    target);
            }

            public ValueTask<DataRow[]> SelectByStartsWithAsync(
                IDbTransaction transaction,
                string startsWith)
            {
                return transaction.ExecuteReaderAsync(
                    () => SelectByStartsWithPathCommand,
                    $"{nameof(DatabaseTable)}/{nameof(SelectByStartsWithPathCommand)}/{TableName}",
                    HandleReaderDataRows,
                    SqliteTransaction.EscapeLikeContent(startsWith) + "%");
            }

            public ValueTask<AttachedDataDataRow[]> SelectAttachedDataByStartsWithAsync(
                IDbTransaction transaction,
                string startsWith)
            {
                return transaction.ExecuteReaderAsync(
                    () => SelectAttachedDataByStartsWithPathCommand,
                    $"{nameof(DatabaseTable)}/{nameof(SelectAttachedDataByStartsWithPathCommand)}/{TableName}",
                    HandleReaderAttachedDataDataRows,
                    SqliteTransaction.EscapeLikeContent(startsWith) + "%");
            }

            /// <summary>
            ///     TODO: use sqlite returning statement.
            ///     https://github.com/ericsink/SQLitePCL.raw/issues/416.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="startsWith">The start string to deleted.</param>
            public ValueTask<int> DeleteByStartsWithAsync(IDbTransaction transaction, string startsWith)
            {
                return transaction.ExecuteNonQueryAsync(
                    () => DeleteByStartsWithPathCommand,
                    $"{nameof(DatabaseTable)}/{nameof(DeleteByStartsWithPathCommand)}/{TableName}",
                    SqliteTransaction.EscapeLikeContent(startsWith) + "%");
            }

            public ValueTask<int> DeleteByPathAsync(IDbTransaction transaction, string path)
            {
                return transaction.ExecuteNonQueryAsync(
                    () => DeleteByPathCommand,
                    $"{nameof(DatabaseTable)}/{nameof(DeleteByPathCommand)}/{TableName}",
                    path);
            }

            public ValueTask<int> UpdateStatsByIdAsync(
                IDbTransaction transaction,
                long id,
                FileStats stats)
            {
                return UpdateStatsByIdAsync(
                    transaction,
                    id,
                    stats.Hash.ContentTag,
                    stats.CreationTime,
                    stats.LastWriteTime,
                    stats.Size,
                    stats.Type);
            }

            public ValueTask<int> UpdateStatsByIdAsync(
                IDbTransaction transaction,
                long id,
                string contentTag,
                DateTimeOffset creationTime,
                DateTimeOffset lastWriteTime,
                long size,
                FileType type)
            {
                return transaction.ExecuteNonQueryAsync(
                    () => UpdateStatsByIdCommand,
                    $"{nameof(DatabaseTable)}/{nameof(UpdateStatsByIdCommand)}/{TableName}",
                    id,
                    contentTag,
                    creationTime,
                    lastWriteTime,
                    size,
                    type);
            }

            public ValueTask<int> UpdateIdentifierTagByIdAsync(
                IDbTransaction transaction,
                long id,
                FileHandle fileHandle,
                FileStats stats)
            {
                return UpdateIdentifierTagByIdAsync(
                    transaction,
                    id,
                    fileHandle.Identifier,
                    stats.Hash.ContentTag,
                    stats.CreationTime,
                    stats.LastWriteTime,
                    stats.Size,
                    stats.Type);
            }

            public ValueTask<int> UpdateIdentifierTagByIdAsync(
                IDbTransaction transaction,
                long id,
                string identifierTag,
                string contentTag,
                DateTimeOffset creationTime,
                DateTimeOffset lastWriteTime,
                long size,
                FileType type)
            {
                return transaction.ExecuteNonQueryAsync(
                    () => UpdateIdentifierTagByIdCommand,
                    $"{nameof(DatabaseTable)}/{nameof(UpdateIdentifierTagByIdCommand)}/{TableName}",
                    id,
                    identifierTag,
                    contentTag,
                    creationTime,
                    lastWriteTime,
                    size,
                    type);
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
                    reader.GetNullableInt64(2),
                    reader.GetBoolean(3),
                    reader.GetNullableString(4),
                    reader.GetNullableString(5),
                    reader.GetNullableDateTimeOffsetFromUnixTimeMilliseconds(6),
                    reader.GetNullableDateTimeOffsetFromUnixTimeMilliseconds(7),
                    reader.GetNullableInt64(8),
                    reader.GetNullableEnum<FileType>(9));
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
                            reader.GetNullableInt64(2),
                            reader.GetBoolean(3),
                            reader.GetNullableString(4),
                            reader.GetNullableString(5),
                            reader.GetNullableDateTimeOffsetFromUnixTimeMilliseconds(6),
                            reader.GetNullableDateTimeOffsetFromUnixTimeMilliseconds(7),
                            reader.GetNullableInt64(8),
                            reader.GetNullableEnum<FileType>(9)));
                }

                return result.ToArray();
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
                            new FileAttachedData(reader.GetString(2))));
                }

                return result.ToArray();
            }

            public record DataRow(
                long Id,
                string Path,
                long? Parent,
                bool IsDirectory,
                string? IdentifierTag,
                string? ContentTag,
                DateTimeOffset? CreationTime,
                DateTimeOffset? LastWriteTime,
                long? Size,
                FileType? FileType)
            {
                public FileStats? FileStats =>
                    CreationTime != null && LastWriteTime != null && Size != null && FileType != null && ContentTag != null
                        ? new FileStats(CreationTime.Value, LastWriteTime.Value, Size.Value, FileType.Value, new FileHash(ContentTag))
                        : null;

                public FileHandle? FileHandle => IdentifierTag != null ? new FileHandle(IdentifierTag) : null;
            }

            public record AttachedDataDataRow(long Id, long Target, FileAttachedData FileAttachedData);
        }
    }
}
