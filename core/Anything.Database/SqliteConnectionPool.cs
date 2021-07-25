using System;
using Anything.Database.Provider;
using Anything.Utils;
using Microsoft.Data.Sqlite;

namespace Anything.Database
{
    public class SqliteConnectionPool : IDisposable
    {
        private readonly ISqliteConnectionProvider _provider;
        private readonly ObjectPool<SqliteConnection> _readPool;
        private readonly ObjectPool<SqliteConnection> _writePool;

        private bool _disposed;

        public SqliteConnectionPool(int maxWriteSize, int maxReadSize, ISqliteConnectionProvider provider)
        {
            _readPool = new ObjectPool<SqliteConnection>(maxReadSize);
            _writePool = new ObjectPool<SqliteConnection>(maxWriteSize);
            _provider = provider;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ObjectPool<SqliteConnection>.Ref GetWriteConnectionRef(bool allowCreate = false, bool isolated = false)
        {
            var connection = _writePool.GetRef(false);

            return connection ?? new ObjectPool<SqliteConnection>.Ref(
                _writePool, _provider.Make(allowCreate ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite, isolated));
        }

        public ObjectPool<SqliteConnection>.Ref GetReadConnectionRef(bool isolated = false)
        {
            var connection = _readPool.GetRef(false);

            return connection ?? new ObjectPool<SqliteConnection>.Ref(_readPool, _provider.Make(SqliteOpenMode.ReadOnly, isolated));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _readPool.Dispose();
                    _writePool.Dispose();
                }

                _disposed = true;
            }
        }

        ~SqliteConnectionPool()
        {
            Dispose(false);
        }
    }
}
