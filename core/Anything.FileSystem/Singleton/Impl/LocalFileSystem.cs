using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Property;
using Anything.FileSystem.Singleton.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Microsoft.Extensions.Logging;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem.Singleton.Impl;

/// <summary>
///     File system provider, providing files from local.
/// </summary>
public class LocalFileSystem
    : BaseFileSystem, IDisposable
{
    private readonly HintFileTracker.MemoryStorage _hintFileTrackerStorage;
    private readonly HintFileTracker _hintFileTracker;
    private readonly string _rootPath;
    private readonly FileSystemDirectoryWalker.WalkerThread _scannerThread;
    private bool _disposed;

    public LocalFileSystem(
        string rootPath,
        IFileEventService fileEventService,
        ILogger<LocalFileSystem> logger)
    {
        _hintFileTrackerStorage = new HintFileTracker.MemoryStorage();
        _hintFileTracker = new HintFileTracker(_hintFileTrackerStorage, fileEventService, logger);

        _rootPath = rootPath;
        _scannerThread = new FileSystemDirectoryWalker(this, LocalFileHandle.Root.FileHandle).StartWalkerThread(HandleScanner);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override ValueTask<FileHandle> CreateFileHandle(string path)
    {
        if (path == "/")
        {
            return ValueTask.FromResult(LocalFileHandle.Root.FileHandle);
        }

        var stats = StatRealPath(GetRealPath(path));

        return ValueTask.FromResult(new LocalFileHandle(path, stats).FileHandle);
    }

    public override ValueTask<string?> GetRealPath(FileHandle fileHandle)
    {
        return ValueTask.FromResult<string?>(GetRealPath(new LocalFileHandle(fileHandle).Path));
    }

    public override ValueTask<string> GetFileName(FileHandle fileHandle)
    {
        return ValueTask.FromResult(PathLib.Basename(new LocalFileHandle(fileHandle).Path));
    }

    protected override ValueTask<string> GetFilePath(FileHandle fileHandle)
    {
        return ValueTask.FromResult(new LocalFileHandle(fileHandle).Path);
    }

    public override async ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name)
    {
        var parentLocalFileHandle = new LocalFileHandle(parentFileHandle);
        var stats = StatRealPath(GetRealPath(parentLocalFileHandle.Path));

        if (!parentLocalFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(parentFileHandle);
        }

        if (!stats.Type.HasFlag(FileType.Directory))
        {
            throw new FileNotADirectoryException(parentFileHandle);
        }

        var targetPath = PathLib.Join(parentLocalFileHandle.Path, name);
        var targetRealPath = GetRealPath(targetPath);
        var fileType = GetFileType(targetRealPath);

        if (fileType != null)
        {
            throw new FileExistsException($"The file '{targetPath}' existed.");
        }

        Directory.CreateDirectory(targetRealPath);

        var targetStat = StatRealPath(targetRealPath);
        var targetFileHandle = new LocalFileHandle(targetPath, targetStat).FileHandle;
        await IndexFile(targetPath, targetFileHandle, targetStat);

        return targetFileHandle;
    }

    public override async ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var parentLocalFileHandle = new LocalFileHandle(parentFileHandle);

        // check
        if (localFileHandle.Path != PathLib.Join(parentLocalFileHandle.Path, name))
        {
            throw new FileNotFoundException("File handles are inconsistent.");
        }

        var stats = StatRealPath(GetRealPath(localFileHandle.Path));
        var parentStats = StatRealPath(GetRealPath(parentLocalFileHandle.Path));

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        if (!parentLocalFileHandle.IsSameFile(parentStats))
        {
            throw new FileNotFoundException(parentFileHandle);
        }

        var realPath = GetRealPath(localFileHandle.Path);

        if (stats.Type.HasFlag(FileType.Directory))
        {
            if (recursive)
            {
                Directory.Delete(realPath, true);
            }
            else
            {
                throw new FileIsADirectoryException(fileHandle);
            }
        }
        else
        {
            File.Delete(realPath);
        }

        await IndexDeletedFile(localFileHandle.Path, fileHandle);
    }

    public override async ValueTask<IEnumerable<Dirent>> ReadDirectory(FileHandle fileHandle)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var realPath = GetRealPath(localFileHandle.Path);
        var directoryInfo = new DirectoryInfo(realPath);

        if (!directoryInfo.Exists)
        {
            var fileType = GetFileType(realPath);

            if (fileType != null && !fileType.Value.HasFlag(FileType.Directory))
            {
                throw new FileNotADirectoryException(fileHandle);
            }

            throw new FileNotFoundException(fileHandle);
        }

        var stats = GetFileStatFromFileSystemInfo(directoryInfo);

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        var result = directoryInfo.EnumerateFileSystemInfos().Select(info =>
        {
            var entryLocalFileHandle = new LocalFileHandle(
                PathLib.Join(localFileHandle.Path, info.Name),
                GetFileStatFromFileSystemInfo(info));
            return new Dirent(
                info.Name,
                entryLocalFileHandle.FileHandle,
                GetFileStatFromFileSystemInfo(info));
        }).ToImmutableArray();

        await IndexDirectory(localFileHandle.Path, fileHandle, result);

        return result;
    }

    public override async ValueTask<ReadOnlyMemory<byte>> ReadFile(FileHandle fileHandle)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var stats = StatRealPath(GetRealPath(localFileHandle.Path));

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        if (stats.Type.HasFlag(FileType.Directory))
        {
            throw new FileIsADirectoryException(fileHandle);
        }

        var realPath = GetRealPath(localFileHandle.Path);

        await IndexFile(localFileHandle.Path, fileHandle, stats);
        var result = await File.ReadAllBytesAsync(realPath);
        return result;
    }

    public override async ValueTask<FileHandle> Rename(
        FileHandle fileHandle,
        FileHandle oldParentFileHandle,
        string oldName,
        FileHandle newParentFileHandle,
        string newName)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var oldParentLocalFileHandle = new LocalFileHandle(oldParentFileHandle);
        var newParentLocalFileHandle = new LocalFileHandle(newParentFileHandle);

        // check
        if (localFileHandle.Path != PathLib.Join(oldParentLocalFileHandle.Path, oldName))
        {
            throw new FileNotFoundException("File handles are inconsistent.");
        }

        var oldRealPath = GetRealPath(localFileHandle.Path);
        var oldFileStats = StatRealPath(oldRealPath);
        var oldParentStats = StatRealPath(GetRealPath(oldParentLocalFileHandle.Path));
        var newParentStats = StatRealPath(GetRealPath(newParentLocalFileHandle.Path));

        if (!localFileHandle.IsSameFile(oldFileStats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        if (!oldParentLocalFileHandle.IsSameFile(oldParentStats))
        {
            throw new FileNotFoundException(oldParentFileHandle);
        }

        if (!newParentLocalFileHandle.IsSameFile(newParentStats))
        {
            throw new FileNotFoundException(newParentFileHandle);
        }

        if (!newParentStats.Type.HasFlag(FileType.Directory))
        {
            throw new FileNotADirectoryException(newParentFileHandle);
        }

        var newPath = PathLib.Join(newParentLocalFileHandle.Path, newName);
        var newRealPath = GetRealPath(newPath);
        if (GetFileType(newRealPath) != null)
        {
            throw new FileExistsException($"The file '{newPath}' existed.");
        }

        if (oldFileStats.Type.HasFlag(FileType.Directory))
        {
            Directory.Move(oldRealPath, newRealPath);
        }
        else
        {
            File.Move(oldRealPath, newRealPath, false);
        }

        var newFileStats = StatRealPath(newRealPath);
        await IndexDeletedFile(localFileHandle.Path, fileHandle);
        var newFileHandle = new LocalFileHandle(newPath, newFileStats).FileHandle;
        await IndexFile(newPath, new LocalFileHandle(newPath, newFileStats).FileHandle, newFileStats);
        return newFileHandle;
    }

    public override async ValueTask<FileStats> Stat(FileHandle fileHandle)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var filePath = localFileHandle.Path;
        var fileStats = StatRealPath(GetRealPath(filePath));

        if (!localFileHandle.IsSameFile(fileStats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        await IndexFile(filePath, fileHandle, fileStats);
        return fileStats;
    }

    public override async ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var filePath = localFileHandle.Path;
        var stats = StatRealPath(GetRealPath(filePath));

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        if (stats.Type.HasFlag(FileType.Directory))
        {
            throw new FileIsADirectoryException(fileHandle);
        }

        var realPath = GetRealPath(filePath);
        await using var fileStream = File.OpenWrite(realPath);
        await fileStream.WriteAsync(content);
        await fileStream.FlushAsync();

        var newStats = StatRealPath(GetRealPath(filePath));
        await IndexFile(filePath, fileHandle, newStats);
    }

    public override async ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content)
    {
        var parentLocalFileHandle = new LocalFileHandle(parentFileHandle);
        var parentFilePath = parentLocalFileHandle.Path;
        var filePath = PathLib.Join(parentFilePath, name);
        var parentStats = StatRealPath(GetRealPath(parentFilePath));

        if (!parentLocalFileHandle.IsSameFile(parentStats))
        {
            throw new FileNotFoundException(parentFileHandle);
        }

        if (!parentStats.Type.HasFlag(FileType.Directory))
        {
            throw new FileNotADirectoryException(parentFileHandle);
        }

        var realPath = GetRealPath(filePath);
        if (GetFileType(realPath) != null)
        {
            throw new FileExistsException($"The file '{filePath}' existed.");
        }

        await using var fileStream = File.OpenWrite(realPath);
        await fileStream.WriteAsync(content);
        await fileStream.FlushAsync();

        var newStats = StatRealPath(realPath);
        var newFileHandle = new LocalFileHandle(filePath, newStats).FileHandle;
        await IndexFile(parentFilePath, newFileHandle, newStats);

        return newFileHandle;
    }

    public override async ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var stats = StatRealPath(GetRealPath(localFileHandle.Path));

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        if (stats.Type.HasFlag(FileType.Directory))
        {
            throw new FileIsADirectoryException(fileHandle);
        }

        var realPath = GetRealPath(localFileHandle.Path);
        await using var stream = File.Open(realPath, FileMode.Open, FileAccess.Read);

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

    public override async ValueTask SetProperty(
        FileHandle fileHandle,
        string name,
        ReadOnlyMemory<byte> value,
        PropertyFeature feature = PropertyFeature.None)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var stats = StatRealPath(GetRealPath(localFileHandle.Path));

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        await _hintFileTracker.SetProperty(localFileHandle.Path, fileHandle, stats, name, value, feature);
    }

    public override ValueTask<ReadOnlyMemory<byte>?> GetProperty(FileHandle fileHandle, string name)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var stats = StatRealPath(GetRealPath(localFileHandle.Path));

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return _hintFileTracker.GetProperty(localFileHandle.Path, fileHandle, stats, name);
    }

    public override ValueTask RemoveProperty(FileHandle fileHandle, string name)
    {
        var localFileHandle = new LocalFileHandle(fileHandle);
        var stats = StatRealPath(GetRealPath(localFileHandle.Path));

        if (!localFileHandle.IsSameFile(stats))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return _hintFileTracker.RemoveProperty(localFileHandle.Path, fileHandle, stats, name);
    }

    public override IFileSystemWalker CreateWalker(FileHandle rootFileHandle)
    {
        return FileSystemWalkerFactory.CreateGenericWalker(this, rootFileHandle);
    }

    private async ValueTask IndexFile(string path, FileHandle fileHandle, FileStats stats)
    {
        await _hintFileTracker.HintFile(path, fileHandle, stats);
    }

    private async ValueTask IndexDirectory(
        string path,
        FileHandle fileHandle,
        IEnumerable<Dirent> entries)
    {
        await _hintFileTracker.HintDirectory(path, fileHandle, entries.ToImmutableArray());
    }

    private async ValueTask IndexDeletedFile(string path, FileHandle fileHandle)
    {
        await _hintFileTracker.HintDeleted(path, fileHandle);
    }

    private string GetRealPath(string path)
    {
        return Path.Join(_rootPath, PathLib.Resolve(path));
    }

    private string GetPath(string realpath)
    {
        return PathLib.Resolve(Path.GetRelativePath(realpath, _rootPath));
    }

    private FileStats StatRealPath(string realPath)
    {
        var type = GetFileType(realPath);
        if (type == null)
        {
            throw new FileNotFoundException($"The file '{GetPath(realPath)}' not found.");
        }

        FileSystemInfo info = type.Value.HasFlag(FileType.Directory)
            ? new DirectoryInfo(realPath)
            : new FileInfo(realPath);

        return GetFileStatFromFileSystemInfo(info);
    }

    private static FileType GetFileTypeFromFileAttributes(FileAttributes fileAttributes)
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
        if (!info.Exists)
        {
            throw new FileNotFoundException($"The file '{GetPath(info.FullName)}' not found.");
        }

        var fileAttributes = info.Attributes;
        var type = GetFileTypeFromFileAttributes(fileAttributes);

        var size = info is FileInfo fileInfo ? fileInfo.Length : 0;
        DateTimeOffset lastWriteTime = info.LastWriteTimeUtc;
        DateTimeOffset createTime = info.CreationTimeUtc;
        using var sha256 = SHA256.Create();
        var hash = Convert.ToBase64String(
            sha256.ComputeHash(
                Encoding.UTF8.GetBytes($"{lastWriteTime.ToUnixTimeMilliseconds()} + '-' + {size}"))).Substring(0, 7);

        return new FileStats(
            createTime,
            lastWriteTime,
            size,
            type,
            new FileHash(hash));
    }

    private static FileType? GetFileType(string path)
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

    private async Task HandleScanner(FileSystemDirectoryWalker.WalkerItem item)
    {
        await IndexDirectory(PathLib.Resolve("/", item.Path), item.FileHandle, item.Entries);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _hintFileTrackerStorage.Dispose();
                _scannerThread.Dispose();
            }

            _disposed = true;
        }
    }

    private record LocalFileHandle
    {
        private const string RootIdentifier = "__ROOT__";

        private readonly FileType _fileType;

        private readonly bool _isRoot;

        public LocalFileHandle(FileHandle fileHandle)
        {
            if (fileHandle.Identifier == RootIdentifier)
            {
                _isRoot = true;
                _fileType = FileType.Directory;
                Path = "/";
                return;
            }

            var identifier = Convert.FromBase64String(fileHandle.Identifier);
            _fileType = (FileType)BinaryPrimitives.ReadInt32BigEndian(identifier.AsSpan(0, 4));
            Path = Encoding.UTF8.GetString(identifier.AsSpan(4));
            _isRoot = false;
        }

        public LocalFileHandle(string path, FileStats stats)
        {
            if (path == "/")
            {
                Path = path;
                _fileType = FileType.Directory;
                _isRoot = true;
                return;
            }

            Path = path;
            _fileType = stats.Type;
            _isRoot = false;
        }

        public string Path { get; }

        public static LocalFileHandle Root => new(new FileHandle(RootIdentifier));

        public FileHandle FileHandle
        {
            get
            {
                if (_isRoot)
                {
                    return new FileHandle(RootIdentifier);
                }

                var pathBytes = Encoding.UTF8.GetBytes(Path);
                var identifier = new byte[4 + Path.Length];
                BinaryPrimitives.WriteInt32BigEndian(identifier.AsSpan(0, 4), (int)_fileType);
                Array.Copy(pathBytes, 0, identifier, 4, pathBytes.Length);
                return new FileHandle(Convert.ToBase64String(identifier));
            }
        }

        public bool IsSameFile(FileStats stats)
        {
            if (_isRoot)
            {
                return true;
            }

            return _fileType == stats.Type;
        }
    }
}
