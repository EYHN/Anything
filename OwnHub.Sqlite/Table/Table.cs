using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Table
{
    public abstract class Table
    {
        protected abstract string DatabaseDropCommand { get; }

        protected abstract string DatabaseCreateCommand { get; }

        protected string TableName { get; }

        protected Table(string tableName)
        {
            TableName = tableName;
        }

        public virtual async ValueTask CreateAsync(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = DatabaseCreateCommand;
            await command.ExecuteNonQueryAsync();
        }

        public virtual void Create(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = DatabaseCreateCommand;
            command.ExecuteNonQuery();
        }

        public virtual async ValueTask DropAsync(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = DatabaseDropCommand;
            await command.ExecuteNonQueryAsync();
        }

        public virtual void Drop(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = DatabaseDropCommand;
            command.ExecuteNonQuery();
        }
    }
}
