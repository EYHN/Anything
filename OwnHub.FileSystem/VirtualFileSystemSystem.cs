using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;
using OwnHub.FileSystem.Indexer;
using OwnHub.FileSystem.Provider;
using OwnHub.Utils;

namespace OwnHub.FileSystem
{
    /// <summary>
    /// File system abstraction, based on a file system provider, provides more powerful file system functionality.
    /// </summary>
    public class VirtualFileSystemSystem : IFileSystemService
    {
        public IFileSystemProvider FileSystemProvider { get; }

        public IFileIndexer? Indexer { get; }

        public VirtualFileSystemSystem(IFileSystemProvider fileSystemProvider, IFileIndexer? indexer = null)
        {
            FileSystemProvider = fileSystemProvider;
            if (indexer != null)
            {
                Indexer = indexer;
                indexer.OnFileChange += events => OnFileChange?.Invoke(events);
            }
        }

        /// <summary>
        /// Copy a file or directory.
        /// Note that the copy operation may modify the modification and creation times, timestamp behavior depends on the implementation.
        /// </summary>
        /// <param name="source">The existing file location.</param>
        /// <param name="destination">The destination location.</param>
        /// <param name="overwrite">Overwrite existing files.</param>
        /// <exception cref="FileNotFoundException"><paramref name="source"/> or parent of <paramref name="destination"/> doesn't exist.</exception>
        /// <exception cref="FileExistsException">files exists and <paramref name="overwrite"/> is false.</exception>
        /// <exception cref="NoPermissionsException">permissions aren't sufficient.</exception>
        public async ValueTask Copy(Url source, Url destination, bool overwrite)
        {
            var sourceType = await FileSystemProvider.Stat(source);

            if (overwrite)
            {
                try
                {
                    await FileSystemProvider.Delete(destination, true);
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

        private async ValueTask CopyFile(Url source, Url destination)
        {
            var sourceContent = await FileSystemProvider.ReadFile(source);
            await FileSystemProvider.WriteFile(destination, sourceContent, true, false);
        }

        private async ValueTask CopyDirectory(Url source, Url destination)
        {
            var sourceDirectoryContent = await FileSystemProvider.ReadDirectory(source);
            await FileSystemProvider.CreateDirectory(destination);

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

        public ValueTask CreateDirectory(Url url)
        {
            return FileSystemProvider.CreateDirectory(url);
        }

        public ValueTask Delete(Url url, bool recursive)
        {
            return FileSystemProvider.Delete(url, recursive);
        }

        public ValueTask<IEnumerable<KeyValuePair<string, FileStat>>> ReadDirectory(Url url)
        {
            return FileSystemProvider.ReadDirectory(url);
        }

        public ValueTask<byte[]> ReadFile(Url url)
        {
            return FileSystemProvider.ReadFile(url);
        }

        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            return FileSystemProvider.Rename(oldUrl, newUrl, overwrite);
        }

        public ValueTask<FileStat> Stat(Url url)
        {
            return FileSystemProvider.Stat(url);
        }

        public ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            return FileSystemProvider.WriteFile(url, content, create, overwrite);
        }


        public async ValueTask IndexFile(Url url, FileStat? stat = null)
        {
            if (Indexer == null)
            {
                return;
            }

            stat ??= await FileSystemProvider.Stat(url);
            await Indexer.IndexFile(url, stat.ToFileRecord());
        }

        public async ValueTask IndexDirectory(Url url, IEnumerable<KeyValuePair<string, FileStat>> content)
        {
            if (Indexer == null)
            {
                return;
            }

            await Indexer.IndexDirectory(url, content.Select(pair => (pair.Key, pair.Value.ToFileRecord())).ToArray());
        }

        public async ValueTask IndexDeletedFile(Url url)
        {
            if (Indexer == null)
            {
                return;
            }

            await Indexer.IndexFile(url, null);
        }

        public async ValueTask AttachMetadata(Url url, FileMetadata metadata)
        {
            if (Indexer == null)
            {
                return;
            }

            await Indexer.AttachMetadata(url, metadata);
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
    }
}
