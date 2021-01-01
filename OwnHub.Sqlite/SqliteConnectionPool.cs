using Microsoft.Data.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Utils;

namespace OwnHub.Sqlite
{
    public class SqliteConnectionPool
    {
        private readonly ObjectPool<SqliteConnection> readPool;
        private readonly ObjectPool<SqliteConnection> writePool;
        private readonly ISqliteConnectionProvider provider;

        public SqliteConnectionPool(int maxWriteSize, int maxReadSize, ISqliteConnectionProvider provider)
        {
            readPool = new ObjectPool<SqliteConnection>(maxReadSize);
            writePool = new ObjectPool<SqliteConnection>(maxWriteSize);
            this.provider = provider;
        }

        public SqliteConnection GetWriteConnection()
        {
            SqliteConnection? connection = writePool.Get(blocking: false);
            return connection ?? provider.Make(SqliteOpenMode.ReadWrite);
        }

        public SqliteConnection GetReadConnection()
        {
            SqliteConnection? connection = readPool.Get(blocking: false);
            
            return connection ?? provider.Make(SqliteOpenMode.ReadOnly);
        }

        public void ReturnWriteConnection(SqliteConnection connection)
        {
            writePool.Return(connection);
        }

        public void ReturnReadConnection(SqliteConnection connection)
        {
            readPool.Return(connection);
        }
    }
}