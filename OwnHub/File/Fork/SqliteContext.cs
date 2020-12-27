using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OwnHub.Utils
{
    public class SqliteContext
    {
        private readonly SqliteConnectionPool pool;
        private readonly SqliteConnectionFactory factory;

        public SqliteContext(string databaseFile)
        {
            factory = new SqliteConnectionFactory(databaseFile);
            pool = new SqliteConnectionPool(1, 10, factory);
        }

        public async Task Create(Func<SqliteConnection, Task> func)
        {
            SqliteConnection connection = factory.Make(SqliteOpenMode.ReadWriteCreate);
            try
            {
                connection.Open();
                await func(connection);
            }
            finally
            {
                connection.Close();
                pool.ReturnWriteConnection(connection);
            }
        }

        public async Task<T> Create<T>(Func<SqliteConnection, Task<T>> func)
        {
            SqliteConnection connection = factory.Make(SqliteOpenMode.ReadWriteCreate);
            try
            {
                connection.Open();
                return await func(connection);
            }
            finally
            {
                connection.Close();
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
                 connection.Close();
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
                connection.Close();
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
                connection.Close();
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
                connection.Close();
                pool.ReturnReadConnection(connection);
            }
        }

        private SqliteConnection? blobReadConnection;
        public SqliteBlob OpenReadBlob(string tableName, string columnName, long rowid)
        {
            if (blobReadConnection == null)
            {
                blobReadConnection = factory.Make(SqliteOpenMode.ReadOnly);
                blobReadConnection.Open();
            }
            return new SqliteBlob(blobReadConnection, tableName, columnName, rowid, readOnly: true);
        }
        
        private SqliteConnection? blobWriteConnection;
        public SqliteBlob OpenWriteBlob(string tableName, string columnName, long rowid)
        {
            if (blobWriteConnection == null)
            {
                blobWriteConnection = factory.Make(SqliteOpenMode.ReadWrite);
                blobWriteConnection.Open();
            }
            return new SqliteBlob(blobWriteConnection, tableName, columnName, rowid, readOnly: false);
        }
        
    }
}