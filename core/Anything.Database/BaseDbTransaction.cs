using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Anything.Database;

public abstract class BaseDbTransaction : BaseTransaction, IDbTransaction
{
    protected BaseDbTransaction(ITransaction.TransactionMode mode)
        : base(mode)
    {
    }

    /// <inheritdoc />
    public abstract int ExecuteNonQuery(Func<string> sqlInitializer, string name, params object?[] args);

    /// <inheritdoc />
    public abstract T ExecuteReader<T>(
        Func<string> sqlInitializer,
        string name,
        Func<DbDataReader, T> readerFunc,
        params object?[] args);

    /// <inheritdoc />
    public abstract object? ExecuteScalar(Func<string> sqlInitializer, string name, params object?[] args);

    /// <inheritdoc />
    public abstract ValueTask<int> ExecuteNonQueryAsync(Func<string> sqlInitializer, string name, params object?[] args);

    /// <inheritdoc />
    public abstract ValueTask<T> ExecuteReaderAsync<T>(
        Func<string> sqlInitializer,
        string name,
        Func<DbDataReader, T> readerFunc,
        params object?[] args);

    /// <inheritdoc />
    public abstract ValueTask<object?> ExecuteScalarAsync(Func<string> sqlInitializer, string name, params object?[] args);
}
