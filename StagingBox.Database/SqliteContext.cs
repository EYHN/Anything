using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using StagingBox.Database.Provider;
using StagingBox.Utils;

namespace StagingBox.Database
{
    public class SqliteContext
    {
        private readonly SqliteConnectionPool _pool;
        private readonly ISqliteConnectionProvider _provider;
        private readonly bool _autoClose = true;

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
            var connection = _pool.GetWriteConnection(allowCreate: true);
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
            var connectionRef = _pool.GetWriteConnectionRef(allowCreate: true);
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

        private SqliteConnection? _blobReadConnection;

        public SqliteBlob OpenReadBlob(string tableName, string columnName, long rowid)
        {
            if (_blobReadConnection == null)
            {
                _blobReadConnection = _provider.Make(SqliteOpenMode.ReadOnly);
                _blobReadConnection.Open();
            }

            return new SqliteBlob(_blobReadConnection, tableName, columnName, rowid, readOnly: true);
        }

        private SqliteConnection? _blobWriteConnection;

        public SqliteBlob OpenWriteBlob(string tableName, string columnName, long rowid)
        {
            if (_blobWriteConnection == null)
            {
                _blobWriteConnection = _provider.Make(SqliteOpenMode.ReadWrite);
                _blobWriteConnection.Open();
            }

            return new SqliteBlob(_blobWriteConnection, tableName, columnName, rowid, readOnly: false);
        }
    }
}
