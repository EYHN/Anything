using System;
using Anything.Database.Provider;
using Anything.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Anything.Database
{
    public class SqliteContext : IDisposable
    {
        private readonly ILogger? _logger;
        private readonly SqliteConnectionPool _pool;
        private bool _disposed;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _pool.Dispose();
                }

                _disposed = true;
            }
        }

        ~SqliteContext()
        {
            Dispose(false);
        }
    }
}
