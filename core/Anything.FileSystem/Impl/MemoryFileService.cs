using System.Threading;
using Anything.Database;
using Anything.Database.Provider;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;

namespace Anything.FileSystem.Impl
{
    public class MemoryFileService : FileService
    {
        private static int _memoryConnectionSequenceId;
        private readonly DatabaseHintFileTracker _databaseHintFileTracker;
        private readonly VirtualFileSystem _localFileSystem;
        private readonly SqliteContext _trackerSqliteContext;
        private bool _disposed;

        public MemoryFileService(Url rootUrl)
        {
            _trackerSqliteContext = BuildSharedMemorySqliteContext();
            _databaseHintFileTracker = new DatabaseHintFileTracker(_trackerSqliteContext);
            _localFileSystem = new VirtualFileSystem(
                rootUrl,
                new MemoryFileSystemProvider(),
                _databaseHintFileTracker);
            AddFileSystem(rootUrl, _localFileSystem);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    _localFileSystem.Dispose();
                    _databaseHintFileTracker.Dispose();
                    _trackerSqliteContext.Dispose();
                }

                _disposed = true;
            }
        }

        private static SqliteContext BuildSharedMemorySqliteContext()
        {
            var connectionProvider =
                new SharedMemoryConnectionProvider("memory-file-tracker-" + Interlocked.Increment(ref _memoryConnectionSequenceId));
            return new SqliteContext(connectionProvider);
        }
    }
}
