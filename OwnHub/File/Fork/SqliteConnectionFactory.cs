using Microsoft.Data.Sqlite;

namespace OwnHub.Utils
{
    public class SqliteConnectionFactory
    {
        private readonly string databaseFile;
        public SqliteConnectionFactory(string databaseFile)
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