using System.IO;
using Anything.Database;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;

namespace Anything.FileSystem.Impl
{
    public class LocalFileServer : FileService
    {
        private readonly DatabaseHintFileTracker _databaseHintFileTracker;
        private readonly VirtualFileSystem _localFileSystem;
        private readonly SqliteContext _trackerSqliteContext;
        private bool _disposed;

        public LocalFileServer(Url rootUrl, string rootPath, string cachePath)
        {
            Directory.CreateDirectory(cachePath);

            _trackerSqliteContext = new SqliteContext(Path.Join(cachePath, "tracker.db"));
            _databaseHintFileTracker = new DatabaseHintFileTracker(_trackerSqliteContext);
            _localFileSystem = new VirtualFileSystem(
                rootUrl,
                new LocalFileSystemProvider(rootPath),
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
    }
}
