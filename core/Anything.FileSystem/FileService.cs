using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Anything.Utils.Event;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem
{
    public class FileService : Disposable, IFileService
    {
        private readonly List<IDisposable> _disposables = new();
        private readonly EventEmitter<FileEvent[]> _fileEventEmitter = new();
        private readonly EventEmitter<AttachDataEvent[]> _attachDataEventEmitter = new();
        private readonly IDictionary<string, IFileSystem> _fileSystems = new Dictionary<string, IFileSystem>();

        public void AddFileSystem(string @namespace, IFileSystem fileSystem)
        {
            _fileSystems.Add(@namespace, fileSystem);
            _disposables.Add(fileSystem.FileEvent.On(args => _fileEventEmitter.EmitAsync(WarpFileEvent(@namespace, args))));
            _disposables.Add(
                fileSystem.AttachDataEvent.On(args => _attachDataEventEmitter.EmitAsync(WarpAttachDataEvent(@namespace, args))));
        }

        public async ValueTask<FileHandle> CreateFileHandle(Url url)
        {
            if (!_fileSystems.TryGetValue(url.Authority, out var fileSystem))
            {
                throw new FileNotFoundException(url);
            }

            return WarpFileHandle(url.Authority, await fileSystem.CreateFileHandle(url.Path));
        }

        public async ValueTask<string?> GetRealPath(FileHandle fileHandle)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(fileHandle);
            }

            return await fileSystem.GetRealPath(rawFileHandle);
        }

        public async ValueTask<string> GetFileName(FileHandle fileHandle)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(fileHandle);
            }

            return await fileSystem.GetFileName(rawFileHandle);
        }

        public async ValueTask<Url> GetUrl(FileHandle fileHandle)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(fileHandle);
            }

            var path = await fileSystem.GetFilePath(rawFileHandle);

            return new Url("anything", @namespace, path);
        }

        public async ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(parentFileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(parentFileHandle);
            }

            return WarpFileHandle(@namespace, await fileSystem.CreateDirectory(rawFileHandle, name));
        }

        public async ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive)
        {
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
            var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(fileHandle);
            }

            return await fileSystem.Stat(rawFileHandle);
        }

        public async ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(fileHandle);
            }

            await fileSystem.WriteFile(rawFileHandle, content);
        }

        public async ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(parentFileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(parentFileHandle);
            }

            return WarpFileHandle(@namespace, await fileSystem.CreateFile(rawFileHandle, name, content));
        }

        public async ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(fileHandle);
            }

            return await fileSystem.ReadFileStream(rawFileHandle, reader);
        }

        public async ValueTask AttachData(FileHandle fileHandle, FileAttachedData attachedData)
        {
            var (@namespace, rawFileHandle) = UnWarpFileHandle(fileHandle);

            if (!_fileSystems.TryGetValue(@namespace, out var fileSystem))
            {
                throw new FileNotFoundException(fileHandle);
            }

            await fileSystem.AttachData(rawFileHandle, attachedData);
        }

        public Event<FileEvent[]> FileEvent => _fileEventEmitter.Event;

        public Event<AttachDataEvent[]> AttachDataEvent => _attachDataEventEmitter.Event;

        public async ValueTask WaitComplete()
        {
            await Task.WhenAll(_fileSystems.Select(item => item.Value.WaitComplete().AsTask()));
        }

        public async ValueTask WaitFullScan()
        {
            await Task.WhenAll(_fileSystems.Select(item => item.Value.WaitFullScan().AsTask()));
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
                rawWalker.Select(item => new FileSystemWalkerEntry(WarpFileHandle(@namespace, item.FileHandle), item.FileStats, item.Path)));
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
            return fileEvents.Select(e => new FileEvent(e.Type, WarpFileHandle(@namespace, e.FileHandle), e.Stats)).ToArray();
        }

        private static AttachDataEvent[] WarpAttachDataEvent(string @namespace, AttachDataEvent[] attachDataEvents)
        {
            return attachDataEvents.Select(e => new AttachDataEvent(e.Type, WarpFileHandle(@namespace, e.FileHandle), e.AttachedData))
                .ToArray();
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
