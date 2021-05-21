using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Anything.Database.Table
{
    /// <summary>
    /// Design see: "Anything.Tests.Database/Design/TriplesSqlDesignTests.cs".
    /// </summary>
    public class TriplesTable : Table
    {
        /// <inheritdoc/>
        protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            ";

        /// <inheritdoc/>
        protected override string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                Id INTEGER PRIMARY KEY,
                Subject INTEGER NOT NULL,
                Predicate TEXT NOT NULL,
                Object,
                ObjectType TEXT NOT NULL
            );

            -- Unique index
            CREATE UNIQUE INDEX IF NOT EXISTS {TableName}SubjectPredicateConstraintUniqueIndex ON {TableName} (Subject, Predicate);
            CREATE UNIQUE INDEX IF NOT EXISTS {TableName}ObjectConstraintUniqueIndex ON {TableName} (Object) WHERE ObjectType LIKE 'O(_%)';

            -- ObjectType check, should match V(_%) or o(_%)
            CREATE TRIGGER IF NOT EXISTS {TableName}CheckObjectTypeOnInsertTrigger INSERT ON {TableName} WHEN NEW.ObjectType NOT LIKE 'O(_%)' AND NEW.ObjectType NOT LIKE 'V(_%)'
                BEGIN
                SELECT RAISE(FAIL, 'ObjectType does not meet the constraints.');
                END;
            CREATE TRIGGER IF NOT EXISTS {TableName}CheckObjectTypeOnUpdateTrigger UPDATE ON {TableName} WHEN NEW.ObjectType NOT LIKE 'O(_%)' AND NEW.ObjectType NOT LIKE 'V(_%)'
                BEGIN
                SELECT RAISE(FAIL, 'ObjectType does not meet the constraints.');
                END;

            -- Subject check, should existed in Object column.
            CREATE TRIGGER IF NOT EXISTS {TableName}CheckSubjectOnInsertTrigger INSERT ON {TableName} WHEN NEW.Subject IS NOT 0 AND NOT EXISTS (SELECT Id FROM {TableName} WHERE ObjectType LIKE 'O(_%)' AND Object=NEW.Subject)
                BEGIN
                SELECT RAISE(FAIL, 'Subject object not found.');
                END;
            CREATE TRIGGER IF NOT EXISTS {TableName}CheckSubjectOnUpdateTrigger UPDATE ON {TableName} WHEN NEW.Subject IS NOT 0 AND NOT EXISTS (SELECT Id FROM {TableName} WHERE ObjectType LIKE 'O(_%)' AND Object=NEW.Subject)
                BEGIN
                SELECT RAISE(FAIL, 'Subject object not found.');
                END;

            -- Delete check, the object should has no properties
            CREATE TRIGGER IF NOT EXISTS {TableName}CheckOnDelete DELETE ON {TableName} WHEN OLD.ObjectType LIKE 'O(_%)' AND EXISTS (SELECT Id FROM {TableName} WHERE Subject=OLD.Object)
                BEGIN
                SELECT RAISE(FAIL, 'Delete object should has no properties.');
                END;

            -- Update check, the object should has no properties
            CREATE TRIGGER IF NOT EXISTS {TableName}CheckOnUpdate UPDATE ON {TableName} WHEN OLD.ObjectType LIKE 'O(_%)' AND EXISTS (SELECT Id FROM {TableName} WHERE Subject=OLD.Object)
                BEGIN
                SELECT RAISE(FAIL, 'Update object should has no properties.');
                END;

            -- Performance optimization index.
            CREATE INDEX IF NOT EXISTS {TableName}PredicateIndex ON {TableName} (Predicate);
            ";

        private string InsertCommand => $@"
            INSERT INTO {TableName} (Subject, Predicate, Object, ObjectType) VALUES(
                ?1, ?2, ?3, ?4
            );
            ";

        private string InsertOrReplaceCommand => $@"
            INSERT OR REPLACE INTO {TableName} (Subject, Predicate, Object, ObjectType) VALUES(
                ?1, ?2, ?3, ?4
            );
            ";

        private string SelectCommand => $@"
                SELECT Object, ObjectType, Predicate FROM {TableName}
                    WHERE Subject=?1 AND Predicate=?2;
                ";

        private string SelectAllCommand => $@"
                SELECT Object, ObjectType, Predicate FROM {TableName}
                    WHERE Subject=?1;
                ";

        private string DeleteCommand => $@"
                DELETE FROM {TableName}
                    WHERE Subject=?1 AND Predicate=?2;
                ";

        private string DeleteAllCommand => $@"
                DELETE FROM {TableName}
                    WHERE Subject=?1;
                ";

        public TriplesTable(string tableName)
            : base(tableName)
        {
        }

        public async ValueTask InsertAsync(
            IDbTransaction transaction,
            long subject,
            string predicate,
            object obj,
            string objectType)
        {
            await transaction.ExecuteNonQueryAsync(
                () => InsertCommand,
                $"{nameof(TriplesTable)}/{nameof(InsertCommand)}/{TableName}",
                subject,
                predicate,
                obj,
                objectType);
        }

        public void Insert(IDbTransaction transaction, long subject, string predicate, object obj, string objectType)
        {
            transaction.ExecuteNonQuery(
                () => InsertCommand,
                $"{nameof(TriplesTable)}/{nameof(InsertCommand)}/{TableName}",
                subject,
                predicate,
                obj,
                objectType);
        }

        public async ValueTask InsertOrReplaceAsync(
            IDbTransaction transaction,
            long subject,
            string predicate,
            object obj,
            string objectType)
        {
            await transaction.ExecuteNonQueryAsync(
                () => InsertOrReplaceCommand,
                $"{nameof(TriplesTable)}/{nameof(InsertOrReplaceCommand)}/{TableName}",
                subject,
                predicate,
                obj,
                objectType);
        }

        public void InsertOrReplace(
            IDbTransaction transaction,
            long subject,
            string predicate,
            object obj,
            string objectType)
        {
            transaction.ExecuteNonQuery(
                () => InsertOrReplaceCommand,
                $"{nameof(TriplesTable)}/{nameof(InsertOrReplaceCommand)}/{TableName}",
                subject,
                predicate,
                obj,
                objectType);
        }

        public async ValueTask<T> SelectAsync<T>(
            IDbTransaction transaction,
            long subject,
            string? predicate,
            Func<DbDataReader, T> action)
        {
            var task = predicate switch
            {
                null => transaction.ExecuteReaderAsync(
                    () => SelectAllCommand,
                    $"{nameof(TriplesTable)}/{nameof(SelectAllCommand)}/{TableName}",
                    action,
                    subject),
                _ => transaction.ExecuteReaderAsync(
                    () => SelectCommand,
                    $"{nameof(TriplesTable)}/{nameof(SelectCommand)}/{TableName}",
                    action,
                    subject,
                    predicate),
            };

            return await task;
        }

        public T Select<T>(
            IDbTransaction transaction,
            long subject,
            string? predicate,
            Func<DbDataReader, T> action)
        {
            return predicate switch
            {
                null => transaction.ExecuteReader(
                    () => SelectAllCommand,
                    $"{nameof(TriplesTable)}/{nameof(SelectAllCommand)}/{TableName}",
                    action,
                    subject),
                _ => transaction.ExecuteReader(
                    () => SelectCommand,
                    $"{nameof(TriplesTable)}/{nameof(SelectCommand)}/{TableName}",
                    action,
                    subject,
                    predicate),
            };
        }

        public async ValueTask DeleteAsync(IDbTransaction transaction, long subject, string? predicate)
        {
            var task = predicate switch
            {
                null => transaction.ExecuteNonQueryAsync(
                    () => DeleteAllCommand,
                    $"{nameof(TriplesTable)}/{nameof(DeleteAllCommand)}/{TableName}",
                    subject),
                _ => transaction.ExecuteNonQueryAsync(
                    () => DeleteCommand,
                    $"{nameof(TriplesTable)}/{nameof(DeleteCommand)}/{TableName}",
                    subject,
                    predicate),
            };

            await task;
        }

        public void Delete(IDbTransaction transaction, long subject, string? predicate)
        {
            var a = predicate switch
            {
                null => transaction.ExecuteNonQuery(
                    () => DeleteAllCommand,
                    $"{nameof(TriplesTable)}/{nameof(DeleteAllCommand)}/{TableName}",
                    subject),
                _ => transaction.ExecuteNonQuery(
                    () => DeleteCommand,
                    $"{nameof(TriplesTable)}/{nameof(DeleteCommand)}/{TableName}",
                    subject,
                    predicate),
            };
        }
    }
}
