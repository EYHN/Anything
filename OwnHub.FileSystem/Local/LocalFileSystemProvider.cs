using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;
using FileNotFoundException = OwnHub.FileSystem.Exception.FileNotFoundException;

namespace OwnHub.FileSystem.Local
{
    public class LocalFileSystemProvider
        : IFileSystemProvider
    {
        private string _rootPath;

        private string GetRealPath(Uri uri)
        {
            return System.IO.Path.Join(_rootPath, PathUtils.Resolve(uri.AbsolutePath));
        }

        public LocalFileSystemProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public ValueTask Copy(Uri source, Uri destination, bool overwrite)
        {
            var sourceRealPath = GetRealPath(source);
            var destinationRealPath = GetRealPath(destination);

            try
            {
                if (System.IO.Directory.Exists(sourceRealPath))
                {
                    DirectoryCopy(sourceRealPath, destinationRealPath, overwrite);
                }
                else
                {
                    System.IO.File.Copy(sourceRealPath, destinationRealPath, overwrite);
                }
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask CreateDirectory(Uri uri)
        {
            var realPath = GetRealPath(uri);

            try
            {
                System.IO.Directory.CreateDirectory(realPath);
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask Delete(Uri uri, bool recursive)
        {
            var realPath = GetRealPath(uri);

            try
            {
                if (System.IO.Directory.Exists(realPath))
                {
                    System.IO.Directory.Delete(realPath, recursive);
                }
                else
                {
                    System.IO.File.Delete(realPath);
                }
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<IEnumerable<KeyValuePair<string, FileType>>> ReadDirectory(Uri uri)
        {
            var realPath = GetRealPath(uri);

            try
            {
                var directoryInfo = new System.IO.DirectoryInfo(realPath);

                if (!directoryInfo.Exists)
                {
                    throw new System.IO.DirectoryNotFoundException(
                        "Directory does not exist or could not be found: "
                        + realPath);
                }

                return ValueTask.FromResult(
                    directoryInfo.EnumerateFileSystemInfos()
                        .Select(info => new KeyValuePair<string, FileType>(info.Name, GetFileTypeFromFileAttributes(info.Attributes))));
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }
        }

        public async ValueTask<byte[]> ReadFile(Uri uri)
        {
            var realPath = GetRealPath(uri);
            try
            {
                return await System.IO.File.ReadAllBytesAsync(realPath);
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }
        }

        public ValueTask Rename(Uri oldUri, Uri newUri, bool overwrite)
        {
            var oldRealPath = GetRealPath(oldUri);
            var newRealPath = GetRealPath(newUri);

            try
            {
                if (System.IO.Directory.Exists(oldRealPath))
                {
                    System.IO.Directory.Move(oldRealPath, newRealPath);
                }
                else
                {
                    System.IO.File.Move(oldRealPath, newRealPath, overwrite);
                }
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<FileStat> Stat(Uri uri)
        {
            var realPath = GetRealPath(uri);
            try
            {
                var attr = System.IO.File.GetAttributes(realPath);
                var type = GetFileTypeFromFileAttributes(attr);
                System.IO.FileSystemInfo info = type.HasFlag(FileType.Directory)
                    ? new System.IO.DirectoryInfo(realPath)
                    : new System.IO.FileInfo(realPath);
                var size = info is System.IO.FileInfo fileInfo ? fileInfo.Length : 0;
                return ValueTask.FromResult(new FileStat(info.CreationTimeUtc, info.LastWriteTimeUtc, size, type));
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }
        }

        public async ValueTask WriteFile(Uri uri, byte[] content, bool create, bool overwrite)
        {
            var realPath = GetRealPath(uri);
            try
            {
                await System.IO.File.WriteAllBytesAsync(realPath, content);
            }
            catch (System.Exception exception)
            {
                throw new FileSystemException(exception.Message);
            }
        }

        private FileType GetFileTypeFromFileAttributes(System.IO.FileAttributes fileAttributes)
        {
            FileType type = 0;

            if (fileAttributes.HasFlag(System.IO.FileAttributes.Directory))
            {
                type |= FileType.Directory;
            }
            else
            {
                type |= FileType.File;
            }

            if (fileAttributes.HasFlag(System.IO.FileAttributes.ReparsePoint))
            {
                type |= FileType.SymbolicLink;
            }

            return type;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool overWrite)
        {
            // Get the subdirectories for the specified directory.
            var dir = new System.IO.DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new System.IO.DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            System.IO.Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                string tempPath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, overWrite);
            }

            // copying subdirectories, copy them and their contents to new location.
            foreach (var subdir in dirs)
            {
                string tempPath = System.IO.Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, overWrite);
            }
        }
    }
}
