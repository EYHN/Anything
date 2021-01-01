using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Provider
{
    public class SharedMemoryConnectionProvider: ISqliteConnectionProvider
    {
        private readonly string name;
        private readonly string connectionString;
        public SharedMemoryConnectionProvider(string name)
        {
            this.name = name;
            connectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory,
                DataSource = name,
                Cache = SqliteCacheMode.Shared
            }.ToString();
        }

        public SqliteConnection Make(SqliteOpenMode _)
        {
            return new SqliteConnection(connectionString);
        }
    }
}