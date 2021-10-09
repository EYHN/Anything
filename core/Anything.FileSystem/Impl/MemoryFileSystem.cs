using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.FileSystem.Tracker;
using Anything.FileSystem.Walker;
using Anything.Utils;
using Anything.Utils.Event;
using Microsoft.Toolkit.HighPerformance;
using Nito.AsyncEx;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem.Impl
{
    /// <summary>
    ///     File system provider, store files in memory.
    /// </summary>
    public class MemoryFileSystem
        : IFileSystem
    {
        private readonly List<Entity?> _nodes = new(new[] { new Directory(-1, 0, "") });
        private readonly AsyncReaderWriterLock _readerWriterLock = new();
        private readonly BatchEventWorkerEmitter<FileEvent> _fileEventEmitter = new(100);
        private readonly BatchEventWorkerEmitter<AttachDataEvent> _attachDataEventEmitter = new(100);

        private int NextNodeId => _nodes.Count;

        private Directory RootDirectory => (Directory)_nodes[0]!;

        public async ValueTask<FileHandle> CreateFileHandle(string path)
        {
            using (await _readerWriterLock.ReaderLockAsync())
            {
                if (TryGetEntity(SplitPath(path), out var entity))
                {
                    return SerializeFileHandle(entity);
                }

                throw new FileNotFoundException(path);
            }
        }

        public ValueTask<string?> GetRealPath(FileHandle fileHandle)
        {
            return ValueTask.FromResult<string?>(null);
        }

        public async ValueTask<string> GetFileName(FileHandle fileHandle)
        {
            using (await _readerWriterLock.ReaderLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var entity))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                return entity.Name;
            }
        }

        public async ValueTask<string> GetFilePath(FileHandle fileHandle)
        {
            using (await _readerWriterLock.ReaderLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var entity))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                if (entity == RootDirectory)
                {
                    return "/";
                }

                var current = entity;
                var pathParts = new List<string>();

                while (current != RootDirectory)
                {
                    pathParts.Add(current.Name);

                    TryGetEntity(current.ParentId, out var parent);

                    current = (Directory)parent!;
                }

                pathParts.Reverse();
                return '/' + string.Join('/', pathParts);
            }
        }

        public async ValueTask<FileHandle> CreateDirectory(FileHandle parentFileHandle, string name)
        {
            using (await _readerWriterLock.WriterLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(parentFileHandle), out var parent))
                {
                    throw new FileNotFoundException(parentFileHandle);
                }

                if (parent is not Directory parentDirectory)
                {
                    throw new FileNotADirectoryException(parentFileHandle);
                }

                var newDirectoryId = NextNodeId;
                var newDirectory = new Directory(parent.Id, newDirectoryId, name);
                _nodes.Add(newDirectory);
                if (parentDirectory.TryAdd(name, newDirectory.Id) == false)
                {
                    _nodes.RemoveAt(_nodes.Count - 1);
                    throw new FileExistsException($"The file named {name} is existed.");
                }

                var newDirectoryFileHandle = SerializeFileHandle(newDirectory.Id);

                await _fileEventEmitter.EmitAsync(FileSystem.FileEvent.Created(
                    newDirectoryFileHandle,
                    newDirectory.Stats));

                return newDirectoryFileHandle;
            }
        }

        public async ValueTask Delete(FileHandle fileHandle, FileHandle parentFileHandle, string name, bool recursive)
        {
            using (await _readerWriterLock.WriterLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(parentFileHandle), out var parent))
                {
                    throw new FileNotFoundException(parentFileHandle);
                }

                if (parent is not Directory parentDirectory)
                {
                    throw new FileNotADirectoryException(parentFileHandle);
                }

                if (!parentDirectory.TryGetValue(name, out var fileId))
                {
                    throw new FileNotFoundException($"The file named {name} does not found.");
                }

                if (fileId != DeserializeFileHandle(fileHandle))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                var targetFile = _nodes[fileId];

                if (targetFile == null)
                {
                    throw new FileNotFoundException(fileHandle);
                }

                if (recursive == false && targetFile is Directory)
                {
                    throw new FileIsADirectoryException(fileHandle);
                }

                if (targetFile is Directory targetDirectory)
                {
                    await DeleteRecursive(targetDirectory);
                }

                parentDirectory.TryRemove(name, out _);
                _nodes[fileId] = null;

                await _fileEventEmitter.EmitAsync(FileSystem.FileEvent.Deleted(
                    fileHandle,
                    targetFile.Stats));
                await _attachDataEventEmitter.EmitAsync(targetFile.AttachedData.Select(
                    attachedData => FileSystem.AttachDataEvent.Deleted(
                        fileHandle,
                        attachedData)).ToArray());
            }
        }

        private async ValueTask DeleteRecursive(Directory directory)
        {
            foreach (var (name, childId) in directory.GetAll())
            {
                var targetFile = _nodes[childId];
                directory.TryRemove(name, out _);
                _nodes[childId] = null;

                if (targetFile == null)
                {
                    continue;
                }

                if (targetFile is Directory targetDirectory)
                {
                    await DeleteRecursive(targetDirectory);
                }

                var fileHandle = SerializeFileHandle(targetFile);
                await _fileEventEmitter.EmitAsync(FileSystem.FileEvent.Deleted(
                    fileHandle,
                    targetFile.Stats));
                await _attachDataEventEmitter.EmitAsync(targetFile.AttachedData.Select(
                    attachedData => FileSystem.AttachDataEvent.Deleted(
                        fileHandle,
                        attachedData)).ToArray());
            }
        }

        public async ValueTask<IEnumerable<Dirent>> ReadDirectory(FileHandle fileHandle)
        {
            using (await _readerWriterLock.ReaderLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var target))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                if (target is not Directory directory)
                {
                    throw new FileNotADirectoryException(fileHandle);
                }

                return directory.GetAll().Select(entry =>
                {
                    var entity = _nodes[entry.ChildId];
                    if (entity == null)
                    {
                        throw new InvalidOperationException();
                    }

                    return new Dirent(entry.Name, SerializeFileHandle(entity), entity.Stats);
                });
            }
        }

        public async ValueTask<ReadOnlyMemory<byte>> ReadFile(FileHandle fileHandle)
        {
            using (await _readerWriterLock.ReaderLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var target))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                if (target is not File file)
                {
                    throw new FileIsADirectoryException(fileHandle);
                }

                return file.Content;
            }
        }

        public async ValueTask<FileHandle> Rename(
            FileHandle fileHandle,
            FileHandle oldParentFileHandle,
            string oldName,
            FileHandle newParentFileHandle,
            string newName)
        {
            using (await _readerWriterLock.WriterLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(oldParentFileHandle), out var oldParent))
                {
                    throw new FileNotFoundException(oldParentFileHandle);
                }

                if (!TryGetEntity(DeserializeFileHandle(newParentFileHandle), out var newParent))
                {
                    throw new FileNotFoundException(newParentFileHandle);
                }

                if (oldParent is not Directory oldParentDirectory)
                {
                    throw new FileNotADirectoryException(oldParentFileHandle);
                }

                if (newParent is not Directory newParentDirectory)
                {
                    throw new FileNotADirectoryException(newParentFileHandle);
                }

                if (!oldParentDirectory.TryGetValue(oldName, out var fileId))
                {
                    throw new FileNotFoundException($"The file named {oldName} does not found.");
                }

                if (fileId != DeserializeFileHandle(fileHandle))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                var targetFile = _nodes[fileId];

                if (targetFile == null)
                {
                    throw new InvalidOperationException();
                }

                if (newParentDirectory.TryGetValue(newName, out _))
                {
                    throw new FileExistsException($"The file named {newName} existed.");
                }

                _nodes[targetFile.Id] = null;
                targetFile.Id = NextNodeId;
                targetFile.ParentId = newParentDirectory.Id;
                targetFile.Name = newName;
                _nodes.Add(targetFile);
                newParentDirectory.TryAdd(newName, targetFile.Id);
                oldParentDirectory.TryRemove(oldName, out _);
                var newFileHandle = SerializeFileHandle(targetFile.Id);
                await _fileEventEmitter.EmitAsync(FileSystem.FileEvent.Deleted(
                    fileHandle,
                    targetFile.Stats));
                await _fileEventEmitter.EmitAsync(FileSystem.FileEvent.Created(
                    newFileHandle,
                    targetFile.Stats));
                return newFileHandle;
            }
        }

        public async ValueTask<FileStats> Stat(FileHandle fileHandle)
        {
            using (await _readerWriterLock.ReaderLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var target))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                return target.Stats;
            }
        }

        public async ValueTask WriteFile(FileHandle fileHandle, ReadOnlyMemory<byte> content)
        {
            using (await _readerWriterLock.WriterLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var target))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                if (target is not File file)
                {
                    throw new FileIsADirectoryException(fileHandle);
                }

                file.Content = content;

                await _fileEventEmitter.EmitAsync(FileSystem.FileEvent.Changed(
                    fileHandle,
                    file.Stats));
            }
        }

        public async ValueTask<FileHandle> CreateFile(FileHandle parentFileHandle, string name, ReadOnlyMemory<byte> content)
        {
            using (await _readerWriterLock.WriterLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(parentFileHandle), out var parent))
                {
                    throw new FileNotFoundException(parentFileHandle);
                }

                if (parent is not Directory parentDirectory)
                {
                    throw new FileNotADirectoryException(parentFileHandle);
                }

                var newFileId = NextNodeId;
                var newFile = new File(parent.Id, newFileId, name, content);
                _nodes.Add(newFile);
                if (parentDirectory.TryAdd(name, newFile.Id) == false)
                {
                    _nodes[newFileId] = null;
                    throw new FileExistsException($"The file named {name} existed.");
                }

                var newFileHandle = SerializeFileHandle(newFile.Id);
                await _fileEventEmitter.EmitAsync(FileSystem.FileEvent.Created(
                    newFileHandle,
                    newFile.Stats));

                return newFileHandle;
            }
        }

        public async ValueTask<T> ReadFileStream<T>(FileHandle fileHandle, Func<Stream, ValueTask<T>> reader)
        {
            using (await _readerWriterLock.ReaderLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var target))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                if (target is not File file)
                {
                    throw new FileIsADirectoryException(fileHandle);
                }

                await using var stream = file.Content.AsStream();
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
        }

        public async ValueTask AttachData(FileHandle fileHandle, FileAttachedData attachedData)
        {
            using (await _readerWriterLock.WriterLockAsync())
            {
                if (!TryGetEntity(DeserializeFileHandle(fileHandle), out var target))
                {
                    throw new FileNotFoundException(fileHandle);
                }

                target.AttachedData.Add(attachedData);
            }
        }

        public Event<FileEvent[]> FileEvent => _fileEventEmitter.Event;

        public Event<AttachDataEvent[]> AttachDataEvent => _attachDataEventEmitter.Event;

        public async ValueTask WaitComplete()
        {
            await Task.WhenAll(_fileEventEmitter.WaitComplete().AsTask(), _attachDataEventEmitter.WaitComplete().AsTask());
        }

        public ValueTask WaitFullScan()
        {
            return ValueTask.CompletedTask;
        }

        public IFileSystemWalker CreateWalker(FileHandle rootFileHandle)
        {
            return FileSystemWalkerFactory.CreateGenericWalker(this, rootFileHandle);
        }

        private bool TryGetEntity(int id, [MaybeNullWhen(false)] out Entity entity)
        {
            entity = _nodes[id];
            return entity != null;
        }

        private bool TryGetEntity(IEnumerable<string> pathParts, [MaybeNullWhen(false)] out Entity entity)
        {
            Entity current = RootDirectory;
            foreach (var part in pathParts)
            {
                if (current is Directory dir)
                {
                    if (dir.TryGetValue(part, out var nextId) && TryGetEntity(nextId, out var next))
                    {
                        current = next;
                    }
                    else
                    {
                        entity = null;
                        return false;
                    }
                }
                else
                {
                    entity = null;
                    return false;
                }
            }

            entity = current;
            return true;
        }

        private static string[] SplitPath(string path)
        {
            return PathLib.Split(path);
        }

        private static int DeserializeFileHandle(FileHandle fileHandle)
        {
            return Convert.ToInt32(fileHandle.Identifier, CultureInfo.InvariantCulture);
        }

        private static FileHandle SerializeFileHandle(int nodeId)
        {
            return new(nodeId.ToString(CultureInfo.InvariantCulture));
        }

        private static FileHandle SerializeFileHandle(Entity entity)
        {
            return new(entity.Id.ToString(CultureInfo.InvariantCulture));
        }

        #region Entity

        private class Directory : Entity
        {
            private readonly Dictionary<string, int> _children = new();

            public Directory(int parentId, int id, string name)
                : base(parentId, id, name, FileType.Directory)
            {
            }

            public IEnumerable<(string Name, int ChildId)> GetAll()
            {
                foreach (var (name, childId) in _children)
                {
                    yield return (name, childId);
                }
            }

            public bool TryAdd(string key, int value)
            {
                if (_children.TryAdd(key, value))
                {
                    UpdateLastWriteTime();
                    return true;
                }

                return false;
            }

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out int value)
            {
                return _children.TryGetValue(key, out value);
            }

            public bool TryRemove(string key, [MaybeNullWhen(false)] out int value)
            {
                if (_children.Remove(key, out value))
                {
                    UpdateLastWriteTime();
                    return true;
                }

                return false;
            }

            protected override FileHash Hash
            {
                get
                {
                    using var sha256 = SHA256.Create();
                    var binary = new byte[sizeof(long)];
                    BinaryPrimitives.WriteInt64BigEndian(binary, LastWriteTime.ToUnixTimeMilliseconds());
                    return new FileHash(Convert.ToBase64String(
                        sha256.ComputeHash(binary)).Substring(0, 7));
                }
            }
        }

        private class File
            : Entity
        {
            private ReadOnlyMemory<byte> _content;
            private FileHash _hash;

            public File(int parentId, int id, string name, ReadOnlyMemory<byte> content)
                : base(parentId, id, name, FileType.File)
            {
                _content = content;
                _hash = ComputeHash();
            }

            public ReadOnlyMemory<byte> Content
            {
                get => _content;
                set
                {
                    _content = value.ToArray();
                    _hash = ComputeHash();
                    UpdateLastWriteTime();
                }
            }

            public long Size => Content.Length;

            protected override FileHash Hash => _hash;

            private FileHash ComputeHash()
            {
                using var sha256 = SHA256.Create();
                using var contentStream = _content.AsStream();
                return new FileHash(Convert.ToBase64String(sha256.ComputeHash(contentStream)).Substring(0, 7));
            }
        }

        private abstract class Entity
        {
            protected Entity(int parentId, int id, string name, FileType type)
            {
                ParentId = parentId;
                Id = id;
                Name = name;
                CreationTime = DateTimeOffset.UtcNow;
                LastWriteTime = DateTimeOffset.UtcNow;
                Type = type;
            }

            public int ParentId { get; set; }

            public int Id { get; set; }

            public string Name { get; set; }

            public DateTimeOffset CreationTime { get; }

            public DateTimeOffset LastWriteTime { get; set; }

            public FileType Type { get; }

            public FileStats Stats => new(CreationTime, LastWriteTime, this is File file ? file.Size : 0, Type, Hash);

            public List<FileAttachedData> AttachedData { get; } = new();

            protected abstract FileHash Hash { get; }

            public void UpdateLastWriteTime()
            {
                LastWriteTime = DateTimeOffset.Now;
            }
        }

        #endregion
    }
}
