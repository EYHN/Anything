using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Table
{
    public class TriplesSequenceTable : Table
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

        public TriplesSequenceTable(string tableName)
            : base(tableName)
        {
        }

        public async ValueTask InsertAsync(SqliteConnection connection, string name, int initial = 0, bool ignoreIfExist = false)
        {
            var command = connection.CreateCommand();
            var insertCommand = ignoreIfExist ? "INSERT OR IGNORE" : "INSERT";
            command.CommandText = $@"
            {insertCommand} INTO {TableName} (Name, Seq) VALUES($name, $initial);
            ";
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$initial", initial);
            await command.ExecuteNonQueryAsync();
        }

        public void Insert(SqliteConnection connection, string name, int initial = 0, bool ignoreIfExist = false)
        {
            var command = connection.CreateCommand();
            var insertCommand = ignoreIfExist ? "INSERT OR IGNORE" : "INSERT";
            command.CommandText = $@"
            {insertCommand} INTO {TableName} (Name, Seq) VALUES($name, $initial);
            ";
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$initial", initial);
            command.ExecuteNonQuery();
        }

        public async ValueTask<long> IncreaseSeqAsync(SqliteConnection connection, string name)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"
            UPDATE {TableName} SET Seq = Seq + 1 WHERE Name = $name;
            SELECT Seq FROM {TableName} WHERE Name = $name;
            ";
            command.Parameters.AddWithValue("$name", name);
            var seq = await command.ExecuteScalarAsync() as long?;
            return seq == null ? throw new InvalidOperationException() : seq.Value;
        }

        public long IncreaseSeq(SqliteConnection connection, string name)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"
            UPDATE {TableName} SET Seq = Seq + 1 WHERE Name = $name;
            SELECT Seq FROM {TableName} WHERE Name = $name;
            ";
            command.Parameters.AddWithValue("$name", name);
            var seq = command.ExecuteScalar() as long?;
            return seq ?? throw new InvalidOperationException();
        }
    }
}
