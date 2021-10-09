using System;
using Anything.Database;
using Anything.Utils;

namespace Anything.FileSystem.Tracker.Database
{
    public partial class HintFileTracker
    {
        public interface IStorage
        {
            public SqliteContext SqliteContext { get; }
        }

        public class MemoryStorage : Disposable, IStorage
        {
            public SqliteContext SqliteContext { get; }

            public MemoryStorage()
            {
                SqliteContext = new SqliteContext();
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();

                SqliteContext.Dispose();
            }
        }

        public class LocalStorage : Disposable, IStorage
        {
            public SqliteContext SqliteContext { get; }

            public LocalStorage(string dbFile)
            {
                SqliteContext = new SqliteContext(dbFile);
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();

                SqliteContext.Dispose();
            }
        }
    }
}
