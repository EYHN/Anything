using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Property;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Microsoft.Extensions.Logging;
using Nito.Disposables;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem.Singleton;

public class SingletonFileService : SingleDisposable<object?>, IFileService
{
    private readonly Dictionary<string, ISingletonFileSystem> _fileSystems;

    private readonly ILogger<SingletonFileService> _logger;

    public SingletonFileService(
        IEnumerable<FileSystemDescriptor> fileSystemDescriptors,
        IFileEventService eventService,
        ILogger<SingletonFileService> logger,
        IServiceProvider serviceProvider)
        : base(null)
    {
        _fileSystems = new Dictionary<string, ISingletonFileSystem>();
        foreach (var descriptor in fileSystemDescriptors)
        {
            var fileSystemServiceProvider =
                new FileSystemServiceProvider(descriptor, new WrappedFileEventService(descriptor, eventService), serviceProvider);

            _fileSystems.Add(descriptor.NameSpace, descriptor.ImplementationFactory(fileSystemServiceProvider));
        }

        _logger = logger;
    }

    public async ValueTask<FileHandle> CreateFileHandle(Url url)
    {
        _logger.LogTrace("Create file handle by url {Url}", url);
        if (!_fileSystems.TryGetValue(url.Authority, out var fileSystem))
        {
            throw new FileNotFoundException(url);
        }

        return WarpFileHandle(url.Authority, await fileSystem.CreateFileHandle(url));
    }

    public async ValueTask<string?> GetRealPath(FileHandle fileHandle)
    {
        _logger.LogTrace("Get real path by file handle {@fileHandle}", fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return await fileSystem.GetRealPath(rawFileHandle);
    }

    public async ValueTask<string> GetFileName(FileHandle fileHandle)
    {
        _logger.LogTrace("Get file name by file handle {@fileHandle}", fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return await fileSystem.GetFileName(rawFileHandle);
    }

    public async ValueTask<Url> GetUrl(FileHandle fileHandle)
    {
        _logger.LogTrace("Get url by file handle {FileHandle}", fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        var url = await fileSystem.GetUrl(rawFileHandle);

        return new Url("anything", url.Authority, url.Path);
    }

    public async ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name)
    {
        _logger.LogTrace("Create directory by {ParentFileHandle} {Name}", parentFileHandle, name);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(parentFileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(parentFileHandle);
        }

        return WarpFileHandle(@namespace, await fileSystem.CreateDirectory(rawFileHandle, name));
    }

    public async ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive)
    {
        _logger.LogTrace(
            "Delete file by {FileHandle} {ParentFileHandle} {Name} {Recursive}",
            fileHandle,
            parentFileHandle,
            name,
            recursive);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);
        var (parentNamespace, rawParentFileHandle) = UnWarpFileHandle(parentFileHandle);

        if (@namespace != parentNamespace)
        {
            throw new FileSystemException("File handles are inconsistent.");
        }

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        await fileSystem.Delete(rawFileHandle, rawParentFileHandle, name, recursive);
    }

    public async ValueTask<IEnumerable<Dirent>> ReadDirectory(FileHandle fileHandle)
    {
        _logger.LogTrace("Read directory by {@fileHandle}", fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        var result = await fileSystem.ReadDirectory(rawFileHandle);

        return result.Select(dirent => new Dirent(dirent.Name, WarpFileHandle(@namespace, dirent.FileHandle), dirent.Stats));
    }

    public async ValueTask<ReadOnlyMemory<byte>> ReadFile(FileHandle fileHandle)
    {
        _logger.LogTrace("Read file by {@fileHandle}", fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return await fileSystem.ReadFile(rawFileHandle);
    }

    public async ValueTask<FileHandle> Rename(
        FileHandle fileHandle,
        FileHandle oldParentFileHandle,
        string oldName,
        FileHandle newParentFileHandle,
        string newName)
    {
        _logger.LogTrace(
            "Rename file from {FileHandle} {OldParentFileHandle} {OldName} to {NewParentFileHandle} {NewName}",
            fileHandle,
            oldParentFileHandle,
            oldName,
            newParentFileHandle,
            newName);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);
        var (oldParentNamespace, rawOldParentFileHandle) = UnWarpFileHandle(oldParentFileHandle);
        var (newParentNamespace, rawNewParentFileHandle) = UnWarpFileHandle(newParentFileHandle);

        if (@namespace != oldParentNamespace || @namespace != newParentNamespace)
        {
            throw new FileSystemException("File handles are inconsistent.");
        }

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return await fileSystem.Rename(rawFileHandle, rawOldParentFileHandle, oldName, rawNewParentFileHandle, newName);
    }

    public async ValueTask<FileStats> Stat(FileHandle fileHandle)
    {
        _logger.LogTrace("Stat file by {FileHandle}", fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return await fileSystem.Stat(rawFileHandle);
    }

    public async ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content)
    {
        _logger.LogTrace("Write {Length} bytes to file {FileHandle}", content.Length, fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        await fileSystem.WriteFile(rawFileHandle, content);
    }

    public async ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content)
    {
        _logger.LogTrace("Create file {ParentFileHandle} {Name} by {Length} bytes data.", parentFileHandle, name, content);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(parentFileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(parentFileHandle);
        }

        return WarpFileHandle(@namespace, await fileSystem.CreateFile(rawFileHandle, name, content));
    }

    public async ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader)
    {
        _logger.LogTrace("Read file stream by {@fileHandle}.", fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return await fileSystem.ReadFileStream(rawFileHandle, reader);
    }

    public async ValueTask SetProperty(
        FileHandle fileHandle,
        string name,
        ReadOnlyMemory<byte> value,
        PropertyFeature feature = PropertyFeature.None)
    {
        _logger.LogTrace("Set property {@key} {@length} to file {@fileHandle}.", name, value.Length, fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        await fileSystem.SetProperty(rawFileHandle, name, value);
    }

    public async ValueTask<ReadOnlyMemory<byte>?> GetProperty(FileHandle fileHandle, string name)
    {
        _logger.LogTrace("Get property {@key} to file {@fileHandle}.", name, fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        return await fileSystem.GetProperty(rawFileHandle, name);
    }

    public async ValueTask RemoveProperty(FileHandle fileHandle, string name)
    {
        _logger.LogTrace("Remove property {@key} to file {@fileHandle}.", name, fileHandle);

        var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(fileHandle);
        }

        await fileSystem.RemoveProperty(rawFileHandle, name);
    }

    public IFileSystemWalker CreateWalker(FileHandle rootFileHandle)
    {
        var (@namespace, rawFileHandle) = UnWarpFileHandle(rootFileHandle);

        if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
        {
            throw new FileNotFoundException(rootFileHandle);
        }

        var rawWalker = fileSystem.CreateWalker(rawFileHandle);
        return FileSystemWalkerFactory.FromEnumerable(
            rootFileHandle,
            rawWalker.Select(item =>
                new FileSystemWalkerEntry(WarpFileHandle(@namespace, item.FileHandle), item.FileStats, item.Path)));
    }

    private static (string Namespace, FileHandle RawFileHandle) UnWarpFileHandle(FileHandle fileHandle)
    {
        var identifier = fileHandle.Identifier;
        var namespaceEnd = identifier.IndexOf(':', StringComparison.Ordinal);

        if (namespaceEnd == -1)
        {
            throw new FileSystemException("Error File Handle Format.");
        }

        var @namespace = identifier.Substring(0, namespaceEnd);
        var rawFileHandle = new FileHandle(identifier.Substring(namespaceEnd + 1));

        return (@namespace, rawFileHandle);
    }

    private static FileHandle WarpFileHandle(string @namespace, FileHandle rawFileHandle)
    {
        return new FileHandle(@namespace + ':' + rawFileHandle.Identifier);
    }

    private static FileEvent[] WarpFileEvent(string @namespace, FileEvent[] fileEvents)
    {
        return fileEvents.Select(e => new FileEvent(e.Type, WarpFileHandle(@namespace, e.FileHandle))).ToArray();
    }

    protected override void Dispose(object? context)
    {
        foreach (var fileSystem in _fileSystems.Values)
        {
            if (fileSystem is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private class WrappedFileEventService : IFileEventService
    {
        private readonly FileSystemDescriptor _descriptor;
        private readonly IFileEventService _fileEventService;

        public WrappedFileEventService(FileSystemDescriptor descriptor, IFileEventService fileEventService)
        {
            _descriptor = descriptor;
            _fileEventService = fileEventService;
        }

        public ValueTask Emit(IEnumerable<FileEvent> fileEvents)
        {
            return _fileEventService.Emit(WarpFileEvent(_descriptor.NameSpace, fileEvents.ToArray()));
        }

        public ValueTask<IAsyncDisposable> Subscribe(Func<IEnumerable<FileEvent>, ValueTask> cb)
        {
            throw new InvalidOperationException("Not Support Operation.");
        }

        public ValueTask WaitComplete()
        {
            throw new InvalidOperationException("Not Support Operation.");
        }
    }

    private class FileSystemServiceProvider : IServiceProvider
    {
        private readonly IFileEventService _fileEventService;
        private readonly FileSystemDescriptor _fileSystemDescriptor;
        private readonly IServiceProvider _serviceProvider;

        public FileSystemServiceProvider(
            FileSystemDescriptor fileSystemDescriptor,
            IFileEventService fileEventService,
            IServiceProvider serviceProvider)
        {
            _fileSystemDescriptor = fileSystemDescriptor;
            _fileEventService = fileEventService;
            _serviceProvider = serviceProvider;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ILogger) ||
                serviceType == typeof(ILoggerFactory) ||
                (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(ILogger<>)))
            {
                return _serviceProvider.GetService(serviceType);
            }

            if (serviceType == typeof(IFileEventService))
            {
                return _fileEventService;
            }

            if (serviceType == typeof(FileSystemDescriptor))
            {
                return _fileSystemDescriptor;
            }

            return null;
        }
    }
}
