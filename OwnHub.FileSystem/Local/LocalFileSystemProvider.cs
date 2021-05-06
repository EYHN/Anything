using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;
using OwnHub.Utils;
using Directory = System.IO.Directory;
using DirectoryInfo = System.IO.DirectoryInfo;
using File = System.IO.File;
using FileAttributes = System.IO.FileAttributes;
using FileInfo = System.IO.FileInfo;
using FileSystemInfo = System.IO.FileSystemInfo;
using Path = System.IO.Path;

namespace OwnHub.FileSystem.Local
{
    public class LocalFileSystemProvider
        : IFileSystemProvider
    {
        private string _rootPath;

        private string GetRealPath(Url url)
        {
            return Path.Join(_rootPath, PathLib.Resolve(url.Path));
        }

        public LocalFileSystemProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public ValueTask CreateDirectory(Url url)
        {
            var realPath = GetRealPath(url);
            var parentPath = PathLib.Dirname(realPath);
            var parentType = GetFileType(parentPath);

            if (parentType == null || !parentType.Value.HasFlag(FileType.Directory))
            {
                throw new FileNotFoundException(parentPath);
            }

            var fileType = GetFileType(realPath);
            if (fileType != null)
            {
                throw new FileExistsException(url);
            }

            Directory.CreateDirectory(realPath);

            return ValueTask.CompletedTask;
        }

        public ValueTask Delete(Url url, bool recursive)
        {
            var realPath = GetRealPath(url);
            var fileType = GetFileType(realPath);

            if (fileType == null)
            {
                throw new FileNotFoundException();
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
                    throw new FileIsADirectoryException();
                }
            }
            else
            {
                File.Delete(realPath);
                return ValueTask.CompletedTask;
            }
        }

        public ValueTask<IEnumerable<KeyValuePair<string, FileType>>> ReadDirectory(Url url)
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
                    .Select(info => new KeyValuePair<string, FileType>(info.Name, GetFileTypeFromFileAttributes(info.Attributes))));
        }

        public async ValueTask<byte[]> ReadFile(Url url)
        {
            var realPath = GetRealPath(url);
            var fileType = GetFileType(realPath);

            if (fileType == null)
            {
                throw new FileNotFoundException();
            }

            if (fileType.Value.HasFlag(FileType.Directory))
            {
                throw new FileIsADirectoryException();
            }

            return await File.ReadAllBytesAsync(realPath);
        }

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
                throw new FileNotFoundException(newParentRealPath);
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

        public ValueTask<FileStat> Stat(Url url)
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
            return ValueTask.FromResult(new FileStat(info.CreationTimeUtc, info.LastWriteTimeUtc, size, type.Value));
        }

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
                    throw new FileIsADirectoryException();
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
            catch (System.IO.DirectoryNotFoundException)
            {
                return null;
            }
        }
    }
}
