using Microsoft.Data.Sqlite;

namespace OwnHub.Utils
{
    public class SqliteConnectionPool
    {
        private readonly ObjectPool<SqliteConnection> readPool;
        private readonly ObjectPool<SqliteConnection> writePool;
        private readonly SqliteConnectionFactory factory;

        public SqliteConnectionPool(int maxWriteSize, int maxReadSize, SqliteConnectionFactory factory)
        {
            readPool = new ObjectPool<SqliteConnection>(maxReadSize);
            writePool = new ObjectPool<SqliteConnection>(maxWriteSize);
            this.factory = factory;
        }

        public SqliteConnection GetWriteConnection()
        {
            SqliteConnection? connection = writePool.Get(blocking: false);
            return connection ?? factory.Make(SqliteOpenMode.ReadWrite);
        }

        public SqliteConnection GetReadConnection()
        {
            SqliteConnection? connection = readPool.Get(blocking: false);
            
            return connection ?? factory.Make(SqliteOpenMode.ReadOnly);
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