using Microsoft.Data.Sqlite;

namespace Anything.Database.Provider
{
    public class SharedMemoryConnectionProvider : ISqliteConnectionProvider
    {
        private readonly string _connectionString;

        public SharedMemoryConnectionProvider(string name)
        {
            Name = name;
            _connectionString = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.Memory, DataSource = name, Cache = SqliteCacheMode.Shared, RecursiveTriggers = true
            }.ToString();
        }

        public string Name { get; }

        public SqliteConnection Make(SqliteOpenMode mode)
        {
            return new(_connectionString);
        }
    }
}
