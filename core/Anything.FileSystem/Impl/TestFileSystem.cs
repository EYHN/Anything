using System;
using Anything.Database;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;

namespace Anything.FileSystem.Impl
{
    public class TestFileSystem : WrappedFileSystem
    {
        private readonly DatabaseHintFileTracker _databaseHintFileTracker;
        private readonly SqliteContext _fileTrackerCacheContext;
        private readonly VirtualFileSystem _localFileSystem;

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

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _localFileSystem.Dispose();
            _databaseHintFileTracker.Dispose();
            _fileTrackerCacheContext.Dispose();
        }
    }
}
