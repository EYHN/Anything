using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;
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

        private string GetRealPath(Uri uri)
        {
            return Path.Join(_rootPath, PathUtils.Resolve(uri.AbsolutePath));
        }

        public LocalFileSystemProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public ValueTask CreateDirectory(Uri uri)
        {
            var realPath = GetRealPath(uri);
            var parentPath = PathUtils.Dirname(realPath);
            var parentType = GetFileType(parentPath);

            if (parentType == null || !parentType.Value.HasFlag(FileType.Directory))
            {
                throw new FileNotFoundException(parentPath);
            }

            var fileType = GetFileType(realPath);
            if (fileType != null)
            {
                throw new FileExistsException(uri);
            }

            Directory.CreateDirectory(realPath);

            return ValueTask.CompletedTask;
        }

        public ValueTask Delete(Uri uri, bool recursive)
        {
            var realPath = GetRealPath(uri);
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

        public ValueTask<IEnumerable<KeyValuePair<string, FileType>>> ReadDirectory(Uri uri)
        {
            var realPath = GetRealPath(uri);
            var directoryInfo = new DirectoryInfo(realPath);

            if (!directoryInfo.Exists)
            {
                var fileType = GetFileType(realPath);

                if (fileType != null && !fileType.Value.HasFlag(FileType.Directory))
                {
                    throw new FileNotADirectoryException(uri);
                }

                throw new FileNotFoundException(uri);
            }

            return ValueTask.FromResult(
                directoryInfo.EnumerateFileSystemInfos()
                    .Select(info => new KeyValuePair<string, FileType>(info.Name, GetFileTypeFromFileAttributes(info.Attributes))));
        }

        public async ValueTask<byte[]> ReadFile(Uri uri)
        {
            var realPath = GetRealPath(uri);
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

        public ValueTask Rename(Uri oldUri, Uri newUri, bool overwrite)
        {
            var oldRealPath = GetRealPath(oldUri);
            var newRealPath = GetRealPath(newUri);
            var oldFileType = GetFileType(oldRealPath);

            if (oldFileType == null)
            {
                throw new FileNotFoundException(oldUri);
            }

            var newParentRealPath = PathUtils.Dirname(newRealPath);
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
                    throw new FileExistsException(newUri);
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

        public ValueTask<FileStat> Stat(Uri uri)
        {
            var realPath = GetRealPath(uri);

            var type = GetFileType(realPath);
            if (type == null)
            {
                throw new FileNotFoundException(uri);
            }

            FileSystemInfo info = type.Value.HasFlag(FileType.Directory)
                ? new DirectoryInfo(realPath)
                : new FileInfo(realPath);
            var size = info is FileInfo fileInfo ? fileInfo.Length : 0;
            return ValueTask.FromResult(new FileStat(info.CreationTimeUtc, info.LastWriteTimeUtc, size, type.Value));
        }

        public async ValueTask WriteFile(Uri uri, byte[] content, bool create, bool overwrite)
        {
            var realPath = GetRealPath(uri);
            var parentPath = PathUtils.Dirname(realPath);
            var parentType = GetFileType(parentPath);

            if (parentType == null || !parentType.Value.HasFlag(FileType.Directory))
            {
                throw new FileNotFoundException(uri);
            }

            var fileType = GetFileType(realPath);
            if (fileType == null && create == false)
            {
                throw new FileNotFoundException(uri);
            }

            if (fileType != null)
            {
                if (overwrite == false)
                {
                    throw new FileExistsException(uri);
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
