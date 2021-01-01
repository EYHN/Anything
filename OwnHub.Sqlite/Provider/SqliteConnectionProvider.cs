using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Provider
{
    public class SqliteConnectionProvider: ISqliteConnectionProvider
    {
        private readonly string databaseFile;
        public SqliteConnectionProvider(string databaseFile)
        {
            this.databaseFile = databaseFile;
        }

        public SqliteConnection Make(SqliteOpenMode mode)
        {
            string connectionString = new SqliteConnectionStringBuilder
            {
                Mode = mode,
                DataSource = databaseFile
            }.ToString();
            return new SqliteConnection(connectionString);
        }
    }
}