using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite
{
    public abstract class BaseDbTransaction : BaseTransaction, IDbTransaction
    {
        public BaseDbTransaction(ITransaction.TransactionMode mode)
            : base(mode)
        {
        }

        /// <inheritdoc/>
        public abstract int ExecuteNonQuery(Func<string> sqlInitializer, string name, params object[] args);

        /// <inheritdoc/>
        public abstract T ExecuteReader<T>(Func<string> sqlInitializer, string name, Func<DbDataReader, T> readerFunc, params object[] args);

        /// <inheritdoc/>
        public abstract object? ExecuteScalar(Func<string> sqlInitializer, string name, params object[] args);

        /// <inheritdoc/>
        public abstract Task<int> ExecuteNonQueryAsync(Func<string> sqlInitializer, string name, params object[] args);

        /// <inheritdoc/>
        public abstract Task<T> ExecuteReaderAsync<T>(
            Func<string> sqlInitializer,
            string name,
            Func<DbDataReader, T> readerFunc,
            params object[] args);

        /// <inheritdoc/>
        public abstract Task<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object[] args);
    }
}
