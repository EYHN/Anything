using System.Threading.Tasks;

namespace Anything.Database.Table
{
    public abstract class Table
    {
        protected Table(string tableName)
        {
            TableName = tableName;
        }

        protected abstract string DatabaseCreateCommand { get; }

        protected abstract string DatabaseDropCommand { get; }

        protected string TableName { get; }

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
