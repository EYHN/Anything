using System;
using Anything.Database.Provider;
using Anything.Utils;
using Microsoft.Data.Sqlite;

namespace Anything.Database
{
    public class SqliteConnectionPool : Disposable
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

        public ObjectPool<SqliteConnection>.Ref GetWriteConnectionRef(bool allowCreate = false, bool isolated = false)
        {
            var connection = _writePool.GetRef(false);

            return connection ?? new ObjectPool<SqliteConnection>.Ref(
#pragma warning disable IDISP004
                _writePool, _provider.Make(allowCreate ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite, isolated));
#pragma warning restore IDISP004
        }

        public ObjectPool<SqliteConnection>.Ref GetReadConnectionRef(bool isolated = false)
        {
            var connection = _readPool.GetRef(false);
#pragma warning disable IDISP004
            return connection ?? new ObjectPool<SqliteConnection>.Ref(_readPool, _provider.Make(SqliteOpenMode.ReadOnly, isolated));
#pragma warning restore IDISP004
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _readPool.Dispose();
            _writePool.Dispose();
        }
    }
}
