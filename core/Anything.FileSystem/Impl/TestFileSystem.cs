using System;
using Anything.Database;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;

namespace Anything.FileSystem.Impl
{
    public class TestFileSystem : WrappedFileSystem, IDisposable
    {
        private readonly DatabaseHintFileTracker _databaseHintFileTracker;
        private readonly SqliteContext _fileTrackerCacheContext;
        private readonly VirtualFileSystem _localFileSystem;
        private bool _disposed;

        public TestFileSystem(Url rootUrl, IFileSystemProvider fileSystemProvider)
        {
            _fileTrackerCacheContext = new SqliteContext();
            _databaseHintFileTracker = new DatabaseHintFileTracker(_fileTrackerCacheContext);
            _localFileSystem = new VirtualFileSystem(
                rootUrl,
                fileSystemProvider,
                _databaseHintFileTracker);
            InnerFileSystem = _localFileSystem;
        }

        protected override IFileSystem InnerFileSystem { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _localFileSystem.Dispose();
                    _databaseHintFileTracker.Dispose();
                    _fileTrackerCacheContext.Dispose();
                }

                _disposed = true;
            }
        }

        ~TestFileSystem()
        {
            Dispose(false);
        }
    }
}
