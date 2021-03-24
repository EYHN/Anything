using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Provider
{
    public class SharedMemoryConnectionProvider : ISqliteConnectionProvider
    {
        public string Name { get; }

        private readonly string _connectionString;

        public SharedMemoryConnectionProvider(string name)
        {
            Name = name;
            _connectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory,
                DataSource = name,
                Cache = SqliteCacheMode.Shared,
                RecursiveTriggers = true
            }.ToString();
        }

        public SqliteConnection Make(SqliteOpenMode mode)
        {
            return new SqliteConnection(_connectionString);
        }
    }
}
