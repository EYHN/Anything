﻿using System;
using Anything.Database.Provider;
using Anything.Utils;
using Microsoft.Data.Sqlite;

namespace Anything.Database
{
    public class SqliteContext : IDisposable
    {
        private readonly bool _autoClose = true;
        private readonly SqliteConnectionPool _pool;

        public SqliteContext(string databaseFile)
        {
            var provider = new SqliteConnectionProvider(databaseFile);
            _pool = new SqliteConnectionPool(1, 10, provider);
        }

        public SqliteContext(ISqliteConnectionProvider connectionProvider)
        {
            if (connectionProvider is SharedMemoryConnectionProvider)
            {
                _autoClose = false;
            }

            _pool = new SqliteConnectionPool(1, 10, connectionProvider);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }

        public ObjectPool<SqliteConnection>.Ref GetCreateConnectionRef(bool isolated = false)
        {
            var connectionRef = _pool.GetWriteConnectionRef(true, isolated);
            connectionRef.Value.Open();
            connectionRef.OnReturn += RefReturnCallback;
            return connectionRef;
        }

        public ObjectPool<SqliteConnection>.Ref GetWriteConnectionRef(bool isolated = false)
        {
            var connectionRef = _pool.GetWriteConnectionRef(isolated);
            connectionRef.Value.Open();
            connectionRef.OnReturn += RefReturnCallback;
            return connectionRef;
        }

        public ObjectPool<SqliteConnection>.Ref GetReadConnectionRef(bool isolated = false)
        {
            var connectionRef = _pool.GetReadConnectionRef(isolated);
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
    }
}
