using System;
using System.Threading;
using Anything.Database.Provider;
using Anything.Utils;
using Microsoft.Data.Sqlite;
using Nito.Disposables;

namespace Anything.Database;

public class SqliteContext : SingleDisposable<object?>
{
    private static int _memoryConnectionSequenceId;
    private readonly SqliteConnectionPool _pool;

    public SqliteContext()
        : base(null)
    {
        var provider = BuildSharedMemoryConnectionProvider();
        _pool = new SqliteConnectionPool(1, 10, provider);
    }

    public SqliteContext(string databaseFile)
        : base(null)
    {
        var provider = new SqliteConnectionProvider(databaseFile);
        _pool = new SqliteConnectionPool(1, 10, provider);
    }

    public SqliteContext(ISqliteConnectionProvider connectionProvider)
        : base(null)
    {
        _pool = new SqliteConnectionPool(1, 10, connectionProvider);
    }

    public ObjectPool<SqliteConnection>.Ref GetCreateConnectionRef(bool isolated = false)
    {
        var connectionRef = _pool.GetWriteConnectionRef(true, isolated);
        connectionRef.Value.Open();
        return connectionRef;
    }

    public ObjectPool<SqliteConnection>.Ref GetWriteConnectionRef(bool isolated = false)
    {
        var connectionRef = _pool.GetWriteConnectionRef(false, isolated);
        connectionRef.Value.Open();
        return connectionRef;
    }

    public ObjectPool<SqliteConnection>.Ref GetReadConnectionRef(bool isolated = false)
    {
        var connectionRef = _pool.GetReadConnectionRef(isolated);
        connectionRef.Value.Open();
        return connectionRef;
    }

    public SqliteTransaction StartTransaction(ITransaction.TransactionMode mode, bool isolated = false)
    {
        return new SqliteTransaction(this, mode, isolated);
    }

    private static SharedMemoryConnectionProvider BuildSharedMemoryConnectionProvider()
    {
        return new SharedMemoryConnectionProvider(
            $"memory-{Interlocked.Increment(ref _memoryConnectionSequenceId)}-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
    }

    protected override void Dispose(object? context)
    {
        _pool.Dispose();
    }
}
