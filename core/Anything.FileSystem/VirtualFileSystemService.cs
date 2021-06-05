using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Indexer;
using Anything.FileSystem.Provider;
using Anything.Utils;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem
{
    /// <summary>
    ///     File system abstraction, based on multiple file system providers, provides more powerful file system functionality.
    /// </summary>
    public class VirtualFileSystemService : IFileSystemService
    {
        private readonly Dictionary<string, IFileSystemProvider> _fileSystemProviders = new();

        public VirtualFileSystemService(IFileIndexer? indexer = null)
        {
            if (indexer != null)
            {
                Indexer = indexer;
                indexer.OnFileChange += events => OnFileChange?.Invoke(events);
            }
        }

        public IFileIndexer? Indexer { get; }

        /// <summary>
        ///     Copy a file or directory.
        ///     Note that the copy operation may modify the modification and creation times, timestamp behavior depends on the implementation.
        /// </summary>
        /// <param name="source">The existing file location.</param>
        /// <param name="destination">The destination location.</param>
        /// <param name="overwrite">Overwrite existing files.</param>
        /// <exception cref="FileSystem.Exception.FileNotFoundException">
        ///     <paramref name="source" /> or parent of <paramref name="destination" />
        ///     doesn't exist.
        /// </exception>
        /// <exception cref="FileExistsException">files exists and <paramref name="overwrite" /> is false.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public async ValueTask Copy(Url source, Url destination, bool overwrite)
        {
            var sourceType = await GetFileSystemProvider(source.Authority).Stat(source);

            if (overwrite)
            {
                try
                {
                    await GetFileSystemProvider(destination.Authority).Delete(destination, true);
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
            var provider = GetFileSystemProvider(url.Authority);
            if (provider is LocalFileSystemProvider localProvider)
            {
                return localProvider.GetRealPath(url);
            }

            return null;
        }

        public async ValueTask CreateDirectory(Url url)
        {
            await GetFileSystemProvider(url.Authority).CreateDirectory(url);
            await IndexDirectory(url, new (string, FileStats)[0]);
        }

        public async ValueTask Delete(Url url, bool recursive)
        {
            await GetFileSystemProvider(url.Authority).Delete(url, recursive);
            await IndexDeletedFile(url);
        }

        public async ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            var result = (await GetFileSystemProvider(url.Authority).ReadDirectory(url)).ToArray();
            await IndexDirectory(url, result);
            return result;
        }

        public ValueTask<byte[]> ReadFile(Url url)
        {
            return GetFileSystemProvider(url.Authority).ReadFile(url);
        }

        public async ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            if (oldUrl.Authority != newUrl.Authority)
            {
                throw new NotImplementedException("not in same namespace");
            }

            await GetFileSystemProvider(oldUrl.Authority).Rename(oldUrl, newUrl, overwrite);

            var newFileStat = await GetFileSystemProvider(newUrl.Authority).Stat(newUrl);
            await IndexDeletedFile(oldUrl);
            await IndexFile(newUrl, newFileStat);
        }

        public async ValueTask<FileStats> Stat(Url url)
        {
            var result = await GetFileSystemProvider(url.Authority).Stat(url);

            await IndexFile(url, result);
            return result;
        }

        public async ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            await GetFileSystemProvider(url.Authority).WriteFile(url, content, create, overwrite);

            var newFileStat = await GetFileSystemProvider(url.Authority).Stat(url);
            await IndexFile(url, newFileStat);
        }

        public async ValueTask<Stream> OpenReadFileStream(Url url)
        {
            var fileSystemProvider = GetFileSystemProvider(url.Authority);
            if (fileSystemProvider is IFileSystemProviderSupportStream fileSystemStreamProvider)
            {
                return await fileSystemStreamProvider.OpenReadFileStream(url);
            }

            var data = await fileSystemProvider.ReadFile(url);
            return new MemoryStream(data, false);
        }

        public async ValueTask AttachMetadata(Url url, FileMetadata metadata, bool replace)
        {
            if (Indexer == null)
            {
                return;
            }

            await Indexer.AttachMetadata(url, metadata, replace);
        }

        public async ValueTask<FileMetadata[]> GetMetadata(Url url)
        {
            if (Indexer == null)
            {
                return Array.Empty<FileMetadata>();
            }

            return await Indexer.GetMetadata(url);
        }

        public event Action<FileChangeEvent[]>? OnFileChange;

        public void RegisterFileSystemProvider(string @namespace, IFileSystemProvider fileSystemProvider)
        {
            _fileSystemProviders.Add(@namespace, fileSystemProvider);
        }

        public IFileSystemProvider GetFileSystemProvider(string @namespace)
        {
            return _fileSystemProviders[@namespace];
        }

        private async ValueTask CopyFile(Url source, Url destination)
        {
            var sourceContent = await GetFileSystemProvider(source.Authority).ReadFile(source);
            await GetFileSystemProvider(destination.Authority).WriteFile(destination, sourceContent, true, false);

            var newFileStat = await GetFileSystemProvider(destination.Authority).Stat(destination);
            await IndexFile(destination, newFileStat);
        }

        private async ValueTask CopyDirectory(Url source, Url destination)
        {
            var sourceDirectoryContent = await GetFileSystemProvider(source.Authority).ReadDirectory(source);
            await GetFileSystemProvider(destination.Authority).CreateDirectory(destination);

            var newFileStat = await GetFileSystemProvider(destination.Authority).Stat(destination);
            await IndexFile(destination, newFileStat);

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

        public async ValueTask IndexFile(Url url, FileStats? stat = null)
        {
            if (Indexer == null)
            {
                return;
            }

            stat ??= await GetFileSystemProvider(url.Authority).Stat(url);
            await Indexer.IndexFile(url, stat.ToFileRecord());
        }

        public async ValueTask IndexDirectory(Url url, IEnumerable<(string Name, FileStats Stat)> content)
        {
            if (Indexer == null)
            {
                return;
            }

            await Indexer.IndexDirectory(url, content.Select(pair => (pair.Name, pair.Stat.ToFileRecord())).ToArray());
        }

        public async ValueTask IndexDeletedFile(Url url)
        {
            if (Indexer == null)
            {
                return;
            }

            await Indexer.IndexFile(url, null);
        }
    }
}
