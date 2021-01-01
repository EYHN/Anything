using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Sqlite.Provider;

namespace OwnHub.Sqlite
{
    public class SqliteContext
    {
        private readonly SqliteConnectionPool pool;
        private readonly ISqliteConnectionProvider provider;
        public bool AutoClose = true;

        public SqliteContext(string databaseFile)
        {
            provider = new SqliteConnectionProvider(databaseFile);
            pool = new SqliteConnectionPool(1, 10, provider);
        }
        
        public SqliteContext(ISqliteConnectionProvider connectionProvider)
        {
            if (connectionProvider is SharedMemoryConnectionProvider)
            {
                AutoClose = false;
            }
            provider = connectionProvider;
            pool = new SqliteConnectionPool(1, 10, provider);
        }

        public async Task Create(Func<SqliteConnection, Task> func)
        {
            SqliteConnection connection = provider.Make(SqliteOpenMode.ReadWriteCreate);
            try
            {
                connection.Open();
                await func(connection);
            }
            finally
            {
                if (AutoClose) connection.Close();
                pool.ReturnWriteConnection(connection);
            }
        }

        public async Task<T> Create<T>(Func<SqliteConnection, Task<T>> func)
        {
            SqliteConnection connection = provider.Make(SqliteOpenMode.ReadWriteCreate);
            try
            {
                connection.Open();
                return await func(connection);
            }
            finally
            {
                if (AutoClose) connection.Close();
                pool.ReturnWriteConnection(connection);
            }
        }
        
        public async Task Write(Func<SqliteConnection, Task> func)
        {
             SqliteConnection connection = pool.GetWriteConnection();
             try
             {
                 connection.Open();
                 await func(connection);
             }
             finally
             {
                 if (AutoClose) connection.Close();
                 pool.ReturnWriteConnection(connection);
             }
        }
        
        public async Task<T> Write<T>(Func<SqliteConnection, Task<T>> func)
        {
            SqliteConnection connection = pool.GetWriteConnection();
            try
            {
                connection.Open();
                return await func(connection);
            }
            finally
            {
                if (AutoClose) connection.Close();
                pool.ReturnWriteConnection(connection);
            }
        }
        
        public async Task Read(Func<SqliteConnection, Task> func)
        {
            SqliteConnection connection = pool.GetReadConnection();
            try
            {
                connection.Open();
                await func(connection);
            }
            finally
            {
                if (AutoClose) connection.Close();
                pool.ReturnReadConnection(connection);
            }
        }
        
        public async Task<T> Read<T>(Func<SqliteConnection, Task<T>> func)
        {
            SqliteConnection connection = pool.GetReadConnection();
            try
            {
                connection.Open();
                return await func(connection);
            }
            finally
            {
                if (AutoClose) connection.Close();
                pool.ReturnReadConnection(connection);
            }
        }

        private SqliteConnection? blobReadConnection;
        public SqliteBlob OpenReadBlob(string tableName, string columnName, long rowid)
        {
            if (blobReadConnection == null)
            {
                blobReadConnection = provider.Make(SqliteOpenMode.ReadOnly);
                blobReadConnection.Open();
            }
            return new SqliteBlob(blobReadConnection, tableName, columnName, rowid, readOnly: true);
        }
        
        private SqliteConnection? blobWriteConnection;
        public SqliteBlob OpenWriteBlob(string tableName, string columnName, long rowid)
        {
            if (blobWriteConnection == null)
            {
                blobWriteConnection = provider.Make(SqliteOpenMode.ReadWrite);
                blobWriteConnection.Open();
            }
            return new SqliteBlob(blobWriteConnection, tableName, columnName, rowid, readOnly: false);
        }
        
    }
}