using Microsoft.Data.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Utils;

namespace OwnHub.Sqlite
{
    public class SqliteConnectionPool
    {
        private readonly ObjectPool<SqliteConnection> _readPool;
        private readonly ObjectPool<SqliteConnection> _writePool;
        private readonly ISqliteConnectionProvider _provider;

        public SqliteConnectionPool(int maxWriteSize, int maxReadSize, ISqliteConnectionProvider provider)
        {
            _readPool = new ObjectPool<SqliteConnection>(maxReadSize);
            _writePool = new ObjectPool<SqliteConnection>(maxWriteSize);
            _provider = provider;
        }

        public SqliteConnection GetWriteConnection(bool allowCreate = false)
        {
            var connection = _writePool.Get(blocking: false);
            return connection ?? _provider.Make(allowCreate ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite);
        }

        public SqliteConnection GetReadConnection()
        {
            var connection = _readPool.Get(blocking: false);

            return connection ?? _provider.Make(SqliteOpenMode.ReadOnly);
        }

        public ObjectPool<SqliteConnection>.Ref GetWriteConnectionRef(bool allowCreate = false)
        {
            var connection = _writePool.GetRef(blocking: false);

            return connection ?? new ObjectPool<SqliteConnection>.Ref(_writePool, _provider.Make(allowCreate ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite));
        }

        public ObjectPool<SqliteConnection>.Ref GetReadConnectionRef()
        {
            var connection = _readPool.GetRef(blocking: false);

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