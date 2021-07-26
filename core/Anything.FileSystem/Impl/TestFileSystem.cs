using System;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;

namespace Anything.FileSystem.Impl
{
    public class TestFileSystem : WrappedFileSystem, IDisposable
    {
        private readonly DatabaseHintFileTracker _databaseHintFileTracker;
        private readonly VirtualFileSystem _localFileSystem;
        private bool _disposed;

        public TestFileSystem(Url rootUrl, IFileSystemProvider fileSystemProvider, string? cacheDbPath = null)
        {
            _databaseHintFileTracker = new DatabaseHintFileTracker(cacheDbPath);
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
