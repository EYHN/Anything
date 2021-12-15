using Anything.Database;
using Nito.Disposables;

namespace Anything.FileSystem.Singleton.Tracker;

public partial class HintFileTracker
{
    public interface IStorage
    {
        public SqliteContext SqliteContext { get; }
    }

    public class MemoryStorage : SingleDisposable<object?>, IStorage
    {
        public MemoryStorage()
            : base(null)
        {
            SqliteContext = new SqliteContext();
        }

        public SqliteContext SqliteContext { get; }

        protected override void Dispose(object? context)
        {
            SqliteContext.Dispose();
        }
    }

    public class LocalStorage : SingleDisposable<object?>, IStorage
    {
        public LocalStorage(string dbFile)
            : base(null)
        {
            SqliteContext = new SqliteContext(dbFile);
        }

        public SqliteContext SqliteContext { get; }

        protected override void Dispose(object? context)
        {
            SqliteContext.Dispose();
        }
    }
}
