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

        public virtual async ValueTask CreateAsync(IDbTransaction transaction)
        {
            await transaction.ExecuteNonQueryAsync(() => DatabaseCreateCommand, $@"{TableName}/Create");
        }

        public virtual void Create(IDbTransaction transaction)
        {
            transaction.ExecuteNonQuery(() => DatabaseCreateCommand, $@"{TableName}/Create");
        }

        public virtual async ValueTask DropAsync(IDbTransaction transaction)
        {
            await transaction.ExecuteNonQueryAsync(() => DatabaseDropCommand, $@"{TableName}/Drop");
        }

        public virtual void Drop(IDbTransaction transaction)
        {
            transaction.ExecuteNonQuery(() => DatabaseDropCommand, $@"{TableName}/Drop");
        }
    }
}
