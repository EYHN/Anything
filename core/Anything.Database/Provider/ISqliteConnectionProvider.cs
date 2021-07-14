using Microsoft.Data.Sqlite;

namespace Anything.Database.Provider
{
    public interface ISqliteConnectionProvider
    {
        public SqliteConnection Make(SqliteOpenMode mode, bool isolated = false);
    }
}
