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
                RecursiveTriggers = true
            }.ToString();
            return new SqliteConnection(connectionString);
        }
    }
}
