using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Provider
{
    public class SqliteConnectionProvider : ISqliteConnectionProvider
    {
        private readonly string _databaseFile;

        public SqliteConnectionProvider(string databaseFile)
        {
            _databaseFile = databaseFile;
        }

        public SqliteConnection Make(SqliteOpenMode mode)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                Mode = mode,
                DataSource = _databaseFile,
                RecursiveTriggers = true,
                Cache = SqliteCacheMode.Shared,
            }.ToString();
            var connection = new SqliteConnection(connectionString);
            var initializeCommand = connection.CreateCommand();
            initializeCommand.CommandText = @"
            PRAGMA read_uncommitted=true;
            PRAGMA cache_size=4000;
            ";
            connection.Open();
            initializeCommand.ExecuteNonQuery();
            connection.Close();
            return connection;
        }
    }
}
