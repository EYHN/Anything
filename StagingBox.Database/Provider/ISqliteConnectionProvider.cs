using Microsoft.Data.Sqlite;

namespace StagingBox.Database.Provider
{
    public interface ISqliteConnectionProvider
    {
        public SqliteConnection Make(SqliteOpenMode mode);
    }
}
