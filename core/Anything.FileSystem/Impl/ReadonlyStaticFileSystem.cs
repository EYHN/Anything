using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Anything.Utils.Event;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem.Impl
{
    public class ReadonlyStaticFileSystem : IFileSystem
    {
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly Url _rootUrl;

        public ReadonlyStaticFileSystem(Url rootUrl, IFileSystemProvider fileSystemProvider)
        {
            _rootUrl = rootUrl;
            _fileSystemProvider = fileSystemProvider;
        }

        public ValueTask CreateDirectory(Url url)
        {
            throw new NotSupportedException();
        }

        public ValueTask Delete(Url url, bool recursive)
        {
            throw new NotSupportedException();
        }

        public ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            AssertUrl(url);

            return _fileSystemProvider.ReadDirectory(url);
        }

        public ValueTask<byte[]> ReadFile(Url url)
        {
            AssertUrl(url);

            return _fileSystemProvider.ReadFile(url);
        }

        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            throw new NotSupportedException();
        }

        public ValueTask<FileStats> Stat(Url url)
        {
            AssertUrl(url);

            return _fileSystemProvider.Stat(url);
        }

        public ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            throw new NotSupportedException();
        }

        public async ValueTask ReadFileStream(Url url, Func<Stream, ValueTask> reader)
        {
            await ReadFileStream(url, async stream =>
            {
                await reader(stream);
                return 1;
            });
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

        public Event<FileEvent[]> FileEvent => new EventEmitter<FileEvent[]>().Event;

        public ValueTask AttachData(Url url, FileRecord fileRecord, FileAttachedData data)
        {
            throw new NotSupportedException();
        }

        public ValueTask Copy(Url source, Url destination, bool overwrite)
        {
            throw new NotSupportedException();
        }

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

        public ValueTask WaitComplete()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask WaitFullScan()
        {
            return ValueTask.CompletedTask;
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

        private void AssertUrl(Url url)
        {
            if (!url.StartsWith(_rootUrl))
            {
                throw new FileNotFoundException(url);
            }
        }
    }
}
