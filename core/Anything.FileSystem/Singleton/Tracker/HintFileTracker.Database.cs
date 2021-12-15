using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Database.Extensions;
using Anything.FileSystem.Property;
using IDbTransaction = Anything.Database.IDbTransaction;

namespace Anything.FileSystem.Singleton.Tracker;

public partial class HintFileTracker
{
    /// <summary>
    ///     The database table for <seealso cref="Tracker.HintFileTracker" />.
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
                ContentTag TEXT
            );

            CREATE TABLE IF NOT EXISTS {TableName}Properties (
                Id INTEGER PRIMARY KEY,
                Target INTEGER NOT NULL REFERENCES {TableName}(Id) ON DELETE CASCADE,
                Key TEXT NOT NULL,
                Value BLOB NOT NULL,
                Feature INTEGER
            );

            CREATE UNIQUE INDEX IF NOT EXISTS {TableName}PropertiesTargetKeyUniqueIndex ON {TableName}Properties(Target, Key);
            ";

        /// <inheritdoc />
        protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            DROP TABLE IF EXISTS {TableName}AttachedData;
            ";

        private string InsertCommand => $@"
            INSERT INTO {TableName} (Path, Parent, IsDirectory, IdentifierTag, ContentTag) VALUES(
                ?1, ?2, ?3, ?4, ?5
            );
            SELECT last_insert_rowid();
            ";

        private string InsertOrReplacePropertyCommand => $@"
            INSERT OR REPLACE INTO {TableName}Properties (Target, Key, Value, Feature) VALUES(
                ?1, ?2, ?3, ?4
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

        private string SelectPropertiesByTargetCommand => $@"
            SELECT Id, Target, Key, Value, Feature FROM {TableName}Properties
                    WHERE Target = ?1;
            ";

        private string SelectPropertyByTargetAndKeyCommand => $@"
            SELECT Id, Target, Key, Value, Feature FROM {TableName}Properties
                    WHERE Target = ?1 and Key = ?2;
            ";

        private string SelectByStartsWithPathCommand => $@"
            SELECT Id, Path, Parent, IsDirectory, IdentifierTag, ContentTag FROM {TableName}
                    WHERE Path LIKE ?1 ESCAPE '\';
            ";

        private string DeleteByStartsWithPathCommand => $@"
            DELETE FROM {TableName} WHERE Path LIKE ?1 ESCAPE '\';
            ";

        private string DeleteByPathCommand => $@"
            DELETE FROM {TableName} WHERE Path = ?1;
            ";

        private string DeletePropertyByTargetAndKeyCommand => $@"
            DELETE FROM {TableName}Properties WHERE Target = ?1 AND Key = ?2;
            ";

        private string DeletePropertyByTargetAndFeatureCommand => $@"
            DELETE FROM {TableName}Properties WHERE Target = ?1 AND (Feature & ?2) != 0;
            ";

        private string UpdateStatsByIdCommand => $@"
            UPDATE {TableName} SET ContentTag = ?2 WHERE Id = ?1;
            ";

        private string UpdateIdentifierTagByIdCommand => $@"
            UPDATE {TableName} SET IdentifierTag = ?2, ContentTag = ?3 WHERE Id = ?1;
            ";

        public async ValueTask<long> InsertAsync(
            IDbTransaction transaction,
            string path,
            long? parent,
            bool isDirectory,
            string? identifierTag,
            string? contentTag)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertCommand,
                $"{nameof(DatabaseTable)}/{nameof(InsertCommand)}/{TableName}",
                path,
                parent,
                isDirectory ? 1 : 0,
                identifierTag,
                contentTag))!;
        }

        public async ValueTask<long> InsertOrReplacePropertyAsync(
            IDbTransaction transaction,
            long target,
            string key,
            ReadOnlyMemory<byte> value,
            PropertyFeature? feature)
        {
            return (long)(await transaction.ExecuteScalarAsync(
                () => InsertOrReplacePropertyCommand,
                $"{nameof(DatabaseTable)}/{nameof(InsertOrReplacePropertyCommand)}/{TableName}",
                target,
                key,
                value.ToArray(), // TODO: fix performance
                feature))!;
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

        public ValueTask<PropertyDataRow[]> SelectPropertiesByTargetAsync(
            IDbTransaction transaction,
            long target)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectPropertiesByTargetCommand,
                $"{nameof(DatabaseTable)}/{nameof(SelectPropertiesByTargetCommand)}/{TableName}",
                HandleReaderPropertyDataRows,
                target);
        }

        public ValueTask<PropertyDataRow?> SelectPropertyAsync(
            IDbTransaction transaction,
            long target,
            string key)
        {
            return transaction.ExecuteReaderAsync(
                () => SelectPropertyByTargetAndKeyCommand,
                $"{nameof(DatabaseTable)}/{nameof(SelectPropertyByTargetAndKeyCommand)}/{TableName}",
                HandleReaderSinglePropertyDataRow,
                target,
                key);
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

        public async ValueTask<bool> DeleteProperty(IDbTransaction transaction, long target, string key)
        {
            return await transaction.ExecuteNonQueryAsync(
                () => DeletePropertyByTargetAndKeyCommand,
                $"{nameof(DatabaseTable)}/{nameof(DeletePropertyByTargetAndKeyCommand)}/{TableName}",
                target,
                key) != 0;
        }

        public async ValueTask<bool> DeletePropertyOnFileUpdated(IDbTransaction transaction, long target)
        {
            return await transaction.ExecuteNonQueryAsync(
                () => DeletePropertyByTargetAndFeatureCommand,
                $"{nameof(DatabaseTable)}/{nameof(DeletePropertyByTargetAndFeatureCommand)}/{TableName}",
                target,
                PropertyFeature.AutoDeleteWhenFileUpdate) != 0;
        }

        public ValueTask<int> UpdateStatsByIdAsync(
            IDbTransaction transaction,
            long id,
            string contentTag)
        {
            return transaction.ExecuteNonQueryAsync(
                () => UpdateStatsByIdCommand,
                $"{nameof(DatabaseTable)}/{nameof(UpdateStatsByIdCommand)}/{TableName}",
                id,
                contentTag);
        }

        public ValueTask<int> UpdateIdentifierTagByIdAsync(
            IDbTransaction transaction,
            long id,
            string identifierTag,
            string contentTag)
        {
            return transaction.ExecuteNonQueryAsync(
                () => UpdateIdentifierTagByIdCommand,
                $"{nameof(DatabaseTable)}/{nameof(UpdateIdentifierTagByIdCommand)}/{TableName}",
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
                reader.GetNullableInt64(2),
                reader.GetBoolean(3),
                reader.GetNullableString(4),
                reader.GetNullableString(5));
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
                        reader.GetNullableString(5)));
            }

            return result.ToArray();
        }

        private PropertyDataRow? HandleReaderSinglePropertyDataRow(IDataReader reader)
        {
            if (!reader.Read())
            {
                return null;
            }

            return new PropertyDataRow(
                reader.GetInt64(0),
                reader.GetInt64(1),
                reader.GetString(2),
                new ReadOnlyMemory<byte>((byte[])reader.GetValue(3)),
                reader.GetNullableEnum<PropertyFeature>(4));
        }

        private PropertyDataRow[] HandleReaderPropertyDataRows(IDataReader reader)
        {
            var result = new List<PropertyDataRow>();

            while (reader.Read())
            {
                result.Add(
                    new PropertyDataRow(
                        reader.GetInt64(0),
                        reader.GetInt64(1),
                        reader.GetString(2),
                        new ReadOnlyMemory<byte>((byte[])reader.GetValue(3)),
                        reader.GetNullableEnum<PropertyFeature>(4)));
            }

            return result.ToArray();
        }

        public record DataRow(
            long Id,
            string Path,
            long? Parent,
            bool IsDirectory,
            string? IdentifierTag,
            string? ContentTag)
        {
            public FileHandle? FileHandle => IdentifierTag != null ? new FileHandle(IdentifierTag) : null;
        }

        public record PropertyDataRow(long Id, long Target, string Key, ReadOnlyMemory<byte> Value, PropertyFeature? Feature);
    }
}
