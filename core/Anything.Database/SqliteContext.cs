using System;
using System.Threading.Tasks;
using Anything.Database.Provider;
using Anything.Utils;
using Microsoft.Data.Sqlite;

namespace Anything.Database
{
    public class SqliteContext
    {
        private readonly bool _autoClose = true;
        private readonly SqliteConnectionPool _pool;
        private readonly ISqliteConnectionProvider _provider;

        private SqliteConnection? _blobReadConnection;

        private SqliteConnection? _blobWriteConnection;

        public SqliteContext(string databaseFile)
        {
            _provider = new SqliteConnectionProvider(databaseFile);
            _pool = new SqliteConnectionPool(1, 10, _provider);
        }

        public SqliteContext(ISqliteConnectionProvider connectionProvider)
        {
            if (connectionProvider is SharedMemoryConnectionProvider)
            {
                _autoClose = false;
            }

            _provider = connectionProvider;
            _pool = new SqliteConnectionPool(1, 10, _provider);
        }

        public async ValueTask Create(Func<SqliteConnection, ValueTask> func)
        {
            var connection = _provider.Make(SqliteOpenMode.ReadWriteCreate);
            try
            {
                connection.Open();
                await func(connection);
            }
            finally
            {
                if (_autoClose)
                {
                    connection.Close();
                }

                _pool.ReturnWriteConnection(connection);
            }
        }

        public async ValueTask<T> Create<T>(Func<SqliteConnection, ValueTask<T>> func)
        {
            var connection = _pool.GetWriteConnection(true);
            try
            {
                connection.Open();
                return await func(connection);
            }
            finally
            {
                if (_autoClose)
                {
                    connection.Close();
                }

                _pool.ReturnWriteConnection(connection);
            }
        }

        public async ValueTask Write(Func<SqliteConnection, ValueTask> func)
        {
            var connection = _pool.GetWriteConnection();
            try
            {
                connection.Open();
                await func(connection);
            }
            finally
            {
                if (_autoClose)
                {
                    connection.Close();
                }

                _pool.ReturnWriteConnection(connection);
            }
        }

        public async ValueTask<T> Write<T>(Func<SqliteConnection, ValueTask<T>> func)
        {
            var connection = _pool.GetWriteConnection();
            try
            {
                connection.Open();
                return await func(connection);
            }
            finally
            {
                if (_autoClose)
                {
                    connection.Close();
                }

                _pool.ReturnWriteConnection(connection);
            }
        }

        public async ValueTask Read(Func<SqliteConnection, ValueTask> func)
        {
            var connection = _pool.GetReadConnection();
            try
            {
                connection.Open();
                await func(connection);
            }
            finally
            {
                if (_autoClose)
                {
                    connection.Close();
                }

                _pool.ReturnReadConnection(connection);
            }
        }

        public async ValueTask<T> Read<T>(Func<SqliteConnection, ValueTask<T>> func)
        {
            var connection = _pool.GetReadConnection();
            try
            {
                connection.Open();
                return await func(connection);
            }
            finally
            {
                if (_autoClose)
                {
                    connection.Close();
                }

                _pool.ReturnReadConnection(connection);
            }
        }

        public ObjectPool<SqliteConnection>.Ref GetCreateConnectionRef()
        {
            var connectionRef = _pool.GetWriteConnectionRef(true);
            connectionRef.Value.Open();
            connectionRef.OnReturn += RefReturnCallback;
            return connectionRef;
        }

        public ObjectPool<SqliteConnection>.Ref GetWriteConnectionRef()
        {
            var connectionRef = _pool.GetWriteConnectionRef();
            connectionRef.Value.Open();
            connectionRef.OnReturn += RefReturnCallback;
            return connectionRef;
        }

        public ObjectPool<SqliteConnection>.Ref GetReadConnectionRef()
        {
            var connectionRef = _pool.GetReadConnectionRef();
            connectionRef.Value.Open();
            connectionRef.OnReturn += RefReturnCallback;
            return connectionRef;
        }

        private void RefReturnCallback(SqliteConnection connection)
        {
            if (_autoClose)
            {
                connection.Close();
            }
        }

        public SqliteBlob OpenReadBlob(string tableName, string columnName, long rowid)
        {
            if (_blobReadConnection == null)
            {
                _blobReadConnection = _provider.Make(SqliteOpenMode.ReadOnly);
                _blobReadConnection.Open();
            }

            return new SqliteBlob(_blobReadConnection, tableName, columnName, rowid, true);
        }

        public SqliteBlob OpenWriteBlob(string tableName, string columnName, long rowid)
        {
            if (_blobWriteConnection == null)
            {
                _blobWriteConnection = _provider.Make(SqliteOpenMode.ReadWrite);
                _blobWriteConnection.Open();
            }

            return new SqliteBlob(_blobWriteConnection, tableName, columnName, rowid, false);
        }
    }
}
