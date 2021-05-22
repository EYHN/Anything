using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Anything.Database.Table
{
    public class SequenceTable : Table
    {
        protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            ";

        protected override string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                Name TEXT NOT NULL UNIQUE,
                Seq INTEGER NOT NULL
            );
            ";

        protected string InsertCommand => $@"
            INSERT INTO {TableName} (Name, Seq) VALUES(?1, ?2);
            ";

        protected string InsertOrIgnoreCommand => $@"
            INSERT OR IGNORE INTO {TableName} (Name, Seq) VALUES(?1, ?2);
            ";

        protected string IncreaseSeqCommand => $@"
            UPDATE {TableName} SET Seq = Seq + 1 WHERE Name = ?1;
            SELECT Seq FROM {TableName} WHERE Name = ?1;
            ";

        public SequenceTable(string tableName)
            : base(tableName)
        {
        }

        public async ValueTask InsertAsync(IDbTransaction transaction, string name, int initial = 0, bool ignoreIfExist = false)
        {
            if (ignoreIfExist)
            {
                await transaction.ExecuteNonQueryAsync(
                    () => InsertOrIgnoreCommand,
                    $"{nameof(SequenceTable)}/{nameof(InsertOrIgnoreCommand)}/{TableName}",
                    name,
                    initial);
            }
            else
            {
                await transaction.ExecuteNonQueryAsync(
                    () => InsertCommand,
                    $"{nameof(SequenceTable)}/{nameof(InsertCommand)}/{TableName}",
                    name,
                    initial);
            }
        }

        public void Insert(IDbTransaction transaction, string name, int initial = 0, bool ignoreIfExist = false)
        {
            if (ignoreIfExist)
            {
                transaction.ExecuteNonQuery(
                    () => InsertOrIgnoreCommand,
                    $"{nameof(SequenceTable)}/{nameof(InsertOrIgnoreCommand)}/{TableName}",
                    name,
                    initial);
            }
            else
            {
                transaction.ExecuteNonQuery(
                    () => InsertCommand,
                    $"{nameof(SequenceTable)}/{nameof(InsertCommand)}/{TableName}",
                    name,
                    initial);
            }
        }

        public async ValueTask<long> IncreaseSeqAsync(IDbTransaction transaction, string name)
        {
            var seq = await transaction.ExecuteScalarAsync(
                () => IncreaseSeqCommand,
                $"{nameof(SequenceTable)}/{nameof(IncreaseSeqCommand)}/{TableName}",
                name) as long?;

            return seq ?? throw new InvalidOperationException();
        }

        public long IncreaseSeq(IDbTransaction transaction, string name)
        {
            var seq = transaction.ExecuteScalar(
                () => IncreaseSeqCommand,
                $"{nameof(SequenceTable)}/{nameof(IncreaseSeqCommand)}/{TableName}",
                name) as long?;

            return seq ?? throw new InvalidOperationException();
        }
    }
}
