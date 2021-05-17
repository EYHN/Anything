using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;
using OwnHub.Utils;
using Directory = System.IO.Directory;
using DirectoryInfo = System.IO.DirectoryInfo;
using File = System.IO.File;
using FileAttributes = System.IO.FileAttributes;
using FileInfo = System.IO.FileInfo;
using FileNotFoundException = OwnHub.FileSystem.Exception.FileNotFoundException;
using FileSystemInfo = System.IO.FileSystemInfo;
using Path = System.IO.Path;

namespace OwnHub.FileSystem.Provider
{
    /// <summary>
    /// File system provider, providing files from local.
    /// </summary>
    public class LocalFileSystemProvider
        : IFileSystemProviderSupportStream
    {
        private string _rootPath;

        /// <summary>
        /// Convert url to local file path.
        /// </summary>
        /// <param name="url">The url to be converted.</param>
        public string GetRealPath(Url url)
        {
            return Path.Join(_rootPath, PathLib.Resolve(url.Path));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileSystemProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path of local files.</param>
        public LocalFileSystemProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        /// <inheritdoc/>
        public ValueTask CreateDirectory(Url url)
        {
            var realPath = GetRealPath(url);
            var parentPath = PathLib.Dirname(realPath);
            var parentType = GetFileType(parentPath);

            if (parentType == null || !parentType.Value.HasFlag(FileType.Directory))
            {
                throw new FileNotFoundException(url.Dirname());
            }

            var fileType = GetFileType(realPath);
            if (fileType != null)
            {
                throw new FileExistsException(url);
            }

            Directory.CreateDirectory(realPath);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask Delete(Url url, bool recursive)
        {
            var realPath = GetRealPath(url);
            var fileType = GetFileType(realPath);

            if (fileType == null)
            {
                throw new FileNotFoundException(url);
            }

            if (fileType.Value.HasFlag(FileType.Directory))
            {
                if (recursive)
                {
                    Directory.Delete(realPath, true);
                    return ValueTask.CompletedTask;
                }
                else
                {
                    throw new FileIsADirectoryException(url);
                }
            }
            else
            {
                File.Delete(realPath);
                return ValueTask.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            var realPath = GetRealPath(url);
            var directoryInfo = new DirectoryInfo(realPath);

            if (!directoryInfo.Exists)
            {
                var fileType = GetFileType(realPath);

                if (fileType != null && !fileType.Value.HasFlag(FileType.Directory))
                {
                    throw new FileNotADirectoryException(url);
                }

                throw new FileNotFoundException(url);
            }

            return ValueTask.FromResult(
                directoryInfo.EnumerateFileSystemInfos()
                    .Select(info => (info.Name, GetFileStatFromFileSystemInfo(info))));
        }

        /// <inheritdoc/>
        public async ValueTask<byte[]> ReadFile(Url url)
        {
            var realPath = GetRealPath(url);
            var fileType = GetFileType(realPath);

            if (fileType == null)
            {
                throw new FileNotFoundException(url);
            }

            if (fileType.Value.HasFlag(FileType.Directory))
            {
                throw new FileIsADirectoryException(url);
            }

            return await File.ReadAllBytesAsync(realPath);
        }

        /// <inheritdoc/>
        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            var oldRealPath = GetRealPath(oldUrl);
            var newRealPath = GetRealPath(newUrl);
            var oldFileType = GetFileType(oldRealPath);

            if (oldFileType == null)
            {
                throw new FileNotFoundException(oldUrl);
            }

            var newParentRealPath = PathLib.Dirname(newRealPath);
            var newParentType = GetFileType(newParentRealPath);
            if (newParentType == null || !newParentType.Value.HasFlag(FileType.Directory))
            {
                throw new FileNotFoundException(newUrl.Dirname());
            }

            var newFileType = GetFileType(newRealPath);
            if (newFileType != null)
            {
                if (!overwrite)
                {
                    throw new FileExistsException(newUrl);
                }

                if (newFileType.Value.HasFlag(FileType.Directory))
                {
                    Directory.Delete(newRealPath, true);
                }
                else
                {
                    File.Delete(newRealPath);
                }
            }

            if (oldFileType.Value.HasFlag(FileType.Directory))
            {
                Directory.Move(oldRealPath, newRealPath);
            }
            else
            {
                File.Move(oldRealPath, newRealPath, overwrite);
            }

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask<FileStats> Stat(Url url)
        {
            var realPath = GetRealPath(url);

            var type = GetFileType(realPath);
            if (type == null)
            {
                throw new FileNotFoundException(url);
            }

            FileSystemInfo info = type.Value.HasFlag(FileType.Directory)
                ? new DirectoryInfo(realPath)
                : new FileInfo(realPath);
            var size = info is FileInfo fileInfo ? fileInfo.Length : 0;
            return ValueTask.FromResult(new FileStats(info.CreationTimeUtc, info.LastWriteTimeUtc, size, type.Value));
        }

        /// <inheritdoc/>
        public async ValueTask WriteFile(Url url, byte[] content, bool create, bool overwrite)
        {
            var realPath = GetRealPath(url);
            var parentPath = PathLib.Dirname(realPath);
            var parentType = GetFileType(parentPath);

            if (parentType == null || !parentType.Value.HasFlag(FileType.Directory))
            {
                throw new FileNotFoundException(url);
            }

            var fileType = GetFileType(realPath);
            if (fileType == null && create == false)
            {
                throw new FileNotFoundException(url);
            }

            if (fileType != null)
            {
                if (overwrite == false)
                {
                    throw new FileExistsException(url);
                }

                if (fileType.Value.HasFlag(FileType.Directory))
                {
                    throw new FileIsADirectoryException(url);
                }
            }

            await File.WriteAllBytesAsync(realPath, content);
        }

        private FileType GetFileTypeFromFileAttributes(FileAttributes fileAttributes)
        {
            FileType type = 0;

            if (fileAttributes.HasFlag(FileAttributes.Directory))
            {
                type |= FileType.Directory;
            }
            else
            {
                type |= FileType.File;
            }

            if (fileAttributes.HasFlag(FileAttributes.ReparsePoint))
            {
                type |= FileType.SymbolicLink;
            }

            return type;
        }

        private FileStats GetFileStatFromFileSystemInfo(FileSystemInfo info)
        {
            var fileAttributes = info.Attributes;
            var type = GetFileTypeFromFileAttributes(fileAttributes);

            var size = info is FileInfo fileInfo ? fileInfo.Length : 0;
            return new FileStats(info.CreationTimeUtc, info.LastWriteTimeUtc, size, type);
        }

        private FileType? GetFileType(string path)
        {
            try
            {
                var attr = File.GetAttributes(path);
                return GetFileTypeFromFileAttributes(attr);
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public ValueTask<Stream> OpenReadFileStream(Url url)
        {
            var realPath = GetRealPath(url);
            var fileType = GetFileType(realPath);

            if (fileType == null)
            {
                throw new FileNotFoundException(url);
            }

            if (fileType.Value.HasFlag(FileType.Directory))
            {
                throw new FileIsADirectoryException(url);
            }

            return ValueTask.FromResult(File.Open(realPath, FileMode.Open, FileAccess.Read) as Stream);
        }
    }
}
