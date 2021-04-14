using Microsoft.Data.Sqlite;

namespace OwnHub.Database.Provider
{
    public interface ISqliteConnectionProvider
    {
        public SqliteConnection Make(SqliteOpenMode mode);
    }
}