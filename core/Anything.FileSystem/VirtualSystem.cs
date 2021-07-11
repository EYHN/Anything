using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;
using Anything.Utils.Event;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem
{
    /// <summary>
    ///     File system abstraction, based on multiple file system providers, provides more powerful file system functionality.
    /// </summary>
    public class VirtualSystem : IFileSystem, IHintProvider
    {
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly EventEmitter<Hint> _hintEventEmitter = new();
        private readonly IFileTracker _innerFileTracker;

        public VirtualSystem(IFileSystemProvider fileSystemProvider, SqliteContext sqliteContext)
        {
            _fileSystemProvider = fileSystemProvider;
            _innerFileTracker = new DatabaseFileTracker(this, sqliteContext);
        }

        /// <inheritdoc />
        public async ValueTask Copy(Url source, Url destination, bool overwrite)
        {
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
            var provider = _fileSystemProvider;
            if (provider is LocalFileSystemProvider localProvider)
            {
                return localProvider.GetRealPath(url);
            }

            return null;
        }

        public async ValueTask CreateDirectory(Url url)
        {
            await _fileSystemProvider.CreateDirectory(url);
            await IndexFile(url);
        }

        public async ValueTask Delete(Url url, bool recursive)
        {
            await _fileSystemProvider.Delete(url, recursive);
            await IndexDeletedFile(url);
        }

        public async ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            var result = (await _fileSystemProvider.ReadDirectory(url)).ToArray();
            await IndexDirectory(url, result);
            return result;
        }

        public ValueTask<byte[]> ReadFile(Url url)
        {
            return _fileSystemProvider.ReadFile(url);
        }

        public async ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            if (oldUrl.Authority != newUrl.Authority)
            {
                throw new NotImplementedException("not in same namespace");
            }

            await _fileSystemProvider.Rename(oldUrl, newUrl, overwrite);

            await IndexDeletedFile(oldUrl);
            await IndexFile(newUrl);
        }

        public async ValueTask<FileStats> Stat(Url url)
        {
            var result = await _fileSystemProvider.Stat(url);

            await IndexFile(url, result);
            return result;
        }

        public async ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            await _fileSystemProvider.WriteFile(url, content, create, overwrite);

            await IndexFile(url);
        }

        public async ValueTask<Stream> OpenReadFileStream(Url url)
        {
            var fileSystemProvider = _fileSystemProvider;
            if (fileSystemProvider is IFileSystemProviderSupportStream fileSystemStreamProvider)
            {
                return await fileSystemStreamProvider.OpenReadFileStream(url);
            }

            var data = await fileSystemProvider.ReadFile(url);
            return new MemoryStream(data, false);
        }

        public Event<FileEvent[]> FileEvent => _innerFileTracker.FileEvent;

        public Task AttachData(Url url, FileRecord fileRecord, FileAttachedData data)
        {
            return _innerFileTracker.AttachData(url, fileRecord, data);
        }

        public ValueTask WaitComplete()
        {
            return _innerFileTracker.WaitComplete();
        }

        /// <inheritdoc />
        public Event<Hint> OnHint => _hintEventEmitter.Event;

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
            await _hintEventEmitter.EmitAsync(
                new FileHint(url, FileRecord.FromFileStats(stat)));
        }

        private async ValueTask IndexDirectory(Url url, IEnumerable<(string Name, FileStats Stat)> content)
        {
            await _hintEventEmitter.EmitAsync(
                new DirectoryHint(url, content.Select(pair => (pair.Name, FileRecord.FromFileStats(pair.Stat))).ToArray()));
        }

        private async ValueTask IndexDeletedFile(Url url)
        {
            await _hintEventEmitter.EmitAsync(new DeletedHint(url));
        }
    }
}
