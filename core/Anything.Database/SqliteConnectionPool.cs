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

        public SqliteConnectionPool(int maxWriteSize, int maxReadSize, ISqliteConnectionProvider provider)
        {
            _readPool = new ObjectPool<SqliteConnection>(maxReadSize);
            _writePool = new ObjectPool<SqliteConnection>(maxWriteSize);
            _provider = provider;
        }

        public void Dispose()
        {
            _readPool.Dispose();
            _writePool.Dispose();
        }

        public SqliteConnection GetWriteConnection(bool allowCreate = false)
        {
            var connection = _writePool.Get(false);
            return connection ?? _provider.Make(allowCreate ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite);
        }

        public SqliteConnection GetReadConnection()
        {
            var connection = _readPool.Get(false);

            return connection ?? _provider.Make(SqliteOpenMode.ReadOnly);
        }

        public ObjectPool<SqliteConnection>.Ref GetWriteConnectionRef(bool allowCreate = false)
        {
            var connection = _writePool.GetRef(false);

            return connection ?? new ObjectPool<SqliteConnection>.Ref(
                _writePool, _provider.Make(allowCreate ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite));
        }

        public ObjectPool<SqliteConnection>.Ref GetReadConnectionRef()
        {
            var connection = _readPool.GetRef(false);

            return connection ?? new ObjectPool<SqliteConnection>.Ref(_readPool, _provider.Make(SqliteOpenMode.ReadOnly));
        }

        public void ReturnWriteConnection(SqliteConnection connection)
        {
            _writePool.Return(connection);
        }

        public void ReturnReadConnection(SqliteConnection connection)
        {
            _readPool.Return(connection);
        }
    }
}
