using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Provider
{
    public interface ISqliteConnectionProvider
    {
        public SqliteConnection Make(SqliteOpenMode mode);
    }
}