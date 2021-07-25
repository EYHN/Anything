using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Anything.Utils.Event;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem.Impl
{
    /// <summary>
    ///     File system abstraction, based on multiple file system providers, provides more powerful file system functionality.
    /// </summary>
    public class VirtualFileSystem : IFileSystem, IDisposable
    {
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IHintFileTracker _innerFileTracker;
        private readonly Url _rootUrl;
        private readonly FileSystemProviderDirectoryWalker.WalkerThread _walkerThread;
        private bool _disposed;

        public VirtualFileSystem(Url rootUrl, IFileSystemProvider fileSystemProvider, IHintFileTracker hintFileTracker)
        {
            _rootUrl = rootUrl;
            _fileSystemProvider = fileSystemProvider;
            _innerFileTracker = hintFileTracker;
            _walkerThread = new FileSystemProviderDirectoryWalker(this, rootUrl).StartWalkerThread(HandleWalker);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask Copy(Url source, Url destination, bool overwrite)
        {
            AssertUrl(source);
            AssertUrl(destination);

            var sourceType = await _fileSystemProvider.Stat(source);

            if (overwrite)
            {
                try
                {
                    await _fileSystemProvider.Delete(destination, true);
                }
                catch (FileNotFoundException)
                {
                }
            }

            if (sourceType.Type.HasFlag(FileType.SymbolicLink))
            {
                return;
            }

            if (sourceType.Type.HasFlag(FileType.File))
            {
                await CopyFile(source, destination);
            }
            else if (sourceType.Type.HasFlag(FileType.Directory))
            {
                await CopyDirectory(source, destination);
            }
        }

        /// <inheritdoc />
        public string? ToLocalPath(Url url)
        {
            AssertUrl(url);

            var provider = _fileSystemProvider;
            if (provider is LocalFileSystemProvider localProvider)
            {
                return localProvider.GetRealPath(url);
            }

            return null;
        }

        public async ValueTask CreateDirectory(Url url)
        {
            AssertUrl(url);

            await _fileSystemProvider.CreateDirectory(url);
            await IndexFile(url);
        }

        public async ValueTask Delete(Url url, bool recursive)
        {
            AssertUrl(url);

            await _fileSystemProvider.Delete(url, recursive);
            await IndexDeletedFile(url);
        }

        public async ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            AssertUrl(url);

            var result = (await _fileSystemProvider.ReadDirectory(url)).ToArray();
            await IndexDirectory(url, result);
            return result;
        }

        public ValueTask<byte[]> ReadFile(Url url)
        {
            AssertUrl(url);

            return _fileSystemProvider.ReadFile(url);
        }

        public async ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            AssertUrl(oldUrl);
            AssertUrl(newUrl);

            await _fileSystemProvider.Rename(oldUrl, newUrl, overwrite);

            await IndexDeletedFile(oldUrl);
            await IndexFile(newUrl);
        }

        public async ValueTask<FileStats> Stat(Url url)
        {
            AssertUrl(url);

            var result = await _fileSystemProvider.Stat(url);

            await IndexFile(url, result);
            return result;
        }

        public async ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            AssertUrl(url);

            await _fileSystemProvider.WriteFile(url, content, create, overwrite);

            await IndexFile(url);
        }

        public async ValueTask<T> ReadFileStream<T>(Url url, Func<Stream, ValueTask<T>> reader)
        {
            AssertUrl(url);

            var fileSystemProvider = _fileSystemProvider;
            if (fileSystemProvider is IFileSystemProviderSupportStream fileSystemStreamProvider)
            {
                return await fileSystemStreamProvider.ReadFileStream(url, reader);
            }

            var data = await fileSystemProvider.ReadFile(url);

            await using var stream = new MemoryStream(data, false);

            T result;
            try
            {
                result = await reader(stream);
            }
            catch (System.Exception e)
            {
                throw new AggregateException("Exception from reader", e);
            }

            return result;
        }

        public Event<FileEvent[]> FileEvent => _innerFileTracker.FileEvent;

        public ValueTask AttachData(Url url, FileRecord fileRecord, FileAttachedData data)
        {
            AssertUrl(url);
            return _innerFileTracker.AttachData(url, fileRecord, data);
        }

        public async ValueTask WaitComplete()
        {
            await _innerFileTracker.WaitComplete();
        }

        public async ValueTask WaitFullScan()
        {
            await _walkerThread.WaitFullWalk();
        }

        public IFileSystemWalker CreateWalker(Url rootUrl)
        {
            AssertUrl(rootUrl);
            return FileSystemWalkerFactory.FromEnumerable(rootUrl, EnumerateAllFiles(rootUrl));
        }

        private async IAsyncEnumerable<Url> EnumerateAllFiles(Url rootUrl)
        {
            var directoryWalker = new FileSystemProviderDirectoryWalker(this, rootUrl);
            await foreach (var directory in directoryWalker)
            {
                foreach (var entries in directory.Entries)
                {
                    var url = directory.Url.JoinPath(entries.Name);
                    yield return url;
                }
            }
        }

        private async ValueTask CopyFile(Url source, Url destination)
        {
            var sourceContent = await _fileSystemProvider.ReadFile(source);
            await _fileSystemProvider.WriteFile(destination, sourceContent, true, false);

            var newFileStat = await _fileSystemProvider.Stat(destination);
            await IndexFile(destination, newFileStat);
        }

        private async ValueTask CopyDirectory(Url source, Url destination)
        {
            var sourceDirectoryContent = await _fileSystemProvider.ReadDirectory(source);
            await _fileSystemProvider.CreateDirectory(destination);

            await IndexFile(destination);

            foreach (var (name, stat) in sourceDirectoryContent)
            {
                // TODO: handling symbolic links
                if (stat.Type.HasFlag(FileType.SymbolicLink))
                {
                    continue;
                }

                var itemSourceUrl = source.JoinPath(name);
                var itemDestinationUrl = destination.JoinPath(name);

                if (stat.Type.HasFlag(FileType.Directory))
                {
                    await CopyDirectory(itemSourceUrl, itemDestinationUrl);
                }
                else if (stat.Type.HasFlag(FileType.File))
                {
                    await CopyFile(itemSourceUrl, itemDestinationUrl);
                }
            }
        }

        private async ValueTask IndexFile(Url url, FileStats? stat = null)
        {
            stat ??= await _fileSystemProvider.Stat(url);
            await _innerFileTracker.CommitHint(
                new FileHint(url, FileRecord.FromFileStats(stat)));
        }

        private async ValueTask IndexDirectory(Url url, IEnumerable<(string Name, FileStats Stat)> entries)
        {
            await _innerFileTracker.CommitHint(
                new DirectoryHint(url, entries.Select(pair => (pair.Name, FileRecord.FromFileStats(pair.Stat))).ToImmutableArray()));
        }

        private async ValueTask IndexDeletedFile(Url url)
        {
            await _innerFileTracker.CommitHint(new DeletedHint(url));
        }

        private async Task HandleWalker(FileSystemProviderDirectoryWalker.WalkerItem item)
        {
            await IndexDirectory(item.Url, item.Entries);
        }

        private void AssertUrl(Url url)
        {
            if (!url.StartsWith(_rootUrl))
            {
                throw new FileNotFoundException(url);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _walkerThread.Dispose();
                }

                _disposed = true;
            }
        }

        ~VirtualFileSystem()
        {
            Dispose(false);
        }
    }
}
