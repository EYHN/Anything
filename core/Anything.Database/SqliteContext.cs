using System;
using System.Threading;
using Anything.Database.Provider;
using Anything.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Anything.Database
{
    public class SqliteContext : Disposable
    {
        private static int _memoryConnectionSequenceId;
        private readonly ILogger? _logger;
        private readonly SqliteConnectionPool _pool;

        public SqliteContext(ILogger? logger = null)
        {
            var provider = BuildSharedMemoryConnectionProvider();
            _pool = new SqliteConnectionPool(1, 10, provider);
            _logger = logger;
        }

        public SqliteContext(string databaseFile, ILogger? logger = null)
        {
            var provider = new SqliteConnectionProvider(databaseFile);
            _pool = new SqliteConnectionPool(1, 10, provider);
            _logger = logger;
        }

        public SqliteContext(ISqliteConnectionProvider connectionProvider, ILogger? logger = null)
        {
            _pool = new SqliteConnectionPool(1, 10, connectionProvider);
            _logger = logger;
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
            return new(this, mode, isolated, _logger);
        }

        private static SharedMemoryConnectionProvider BuildSharedMemoryConnectionProvider()
        {
            return new(
                $"memory-file-tracker-{Interlocked.Increment(ref _memoryConnectionSequenceId)}-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _pool.Dispose();
        }
    }
}
