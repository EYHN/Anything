using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;

namespace OwnHub.FileSystem.Memory
{
    public class MemoryFileSystemProvider
        : IFileSystemProvider
    {
        private Directory _rootDirectory = new();

        private static string GetRealPath(Uri uri)
        {
            return PathUtils.Resolve(uri.AbsolutePath);
        }

        private static string[] SplitPath(string path)
        {
            return PathUtils.Split(path);
        }

        private bool TryGetFile(IEnumerable<string> pathParts, [MaybeNullWhen(false)] out Entity entity)
        {
            Entity current = _rootDirectory;
            foreach (var part in pathParts)
            {
                if (current is Directory dir)
                {
                    if (dir.TryGetValue(part, out var next))
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

        public ValueTask Copy(Uri source, Uri destination, bool overwrite)
        {
            var sourcePathParts = SplitPath(GetRealPath(source));
            var destinationPathParts = SplitPath(GetRealPath(destination));

            if (TryGetFile(sourcePathParts, out var sourceEntity))
            {
                if (TryGetFile(destinationPathParts.SkipLast(1), out var destinationParent) &&
                    destinationParent is Directory destinationParentDirectory)
                {
                    if (overwrite)
                    {
                        destinationParentDirectory[destinationPathParts[^1]] = (Entity)sourceEntity.Clone();
                    }
                    else
                    {
                        if (destinationParentDirectory.TryAdd(destinationPathParts[^1], (Entity)sourceEntity.Clone()) == false)
                        {
                            throw new FileExistsException(destination);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException('/' + string.Join('/', destinationPathParts.SkipLast(1)));
                }
            }
            else
            {
                throw new FileNotFoundException(source);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask CreateDirectory(Uri uri)
        {
            var pathParts = SplitPath(GetRealPath(uri));

            if (TryGetFile(pathParts.SkipLast(1), out var parent) && parent is Directory parentDirectory)
            {
                if (parentDirectory.TryAdd(pathParts[^1], new Directory()) == false)
                {
                    throw new FileExistsException('/' + string.Join('/', pathParts));
                }
            }
            else
            {
                throw new FileNotFoundException('/' + string.Join('/', pathParts.SkipLast(1)));
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask Delete(Uri uri, bool recursive)
        {
            var pathParts = SplitPath(GetRealPath(uri));

            if (TryGetFile(pathParts.SkipLast(1), out var parent) && parent is Directory parentDirectory &&
                parentDirectory.TryGetValue(pathParts[^1], out var target))
            {
                if (recursive == false && target is Directory)
                {
                    throw new FileIsADirectoryException('/' + string.Join('/', pathParts));
                }

                if (parentDirectory.Remove(pathParts[^1]) == false)
                {
                    throw new FileSystemException("Failed to delete: " + '/' + string.Join('/', pathParts));
                }
            }
            else
            {
                throw new FileNotFoundException('/' + string.Join('/', pathParts));
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<IEnumerable<KeyValuePair<string, FileType>>> ReadDirectory(Uri uri)
        {
            var pathParts = SplitPath(GetRealPath(uri));

            if (TryGetFile(pathParts, out var target))
            {
                if (target is Directory targetDirectory)
                {
                    return ValueTask.FromResult(
                        targetDirectory.Select(pair => new KeyValuePair<string, FileType>(pair.Key, pair.Value.Type)));
                }
                else
                {
                    throw new FileNotADirectoryException(uri);
                }
            }
            else
            {
                throw new FileNotFoundException(uri);
            }
        }

        public ValueTask<byte[]> ReadFile(Uri uri)
        {
            var pathParts = SplitPath(GetRealPath(uri));

            if (TryGetFile(pathParts, out var target))
            {
                if (target is File targetDirectory)
                {
                    return ValueTask.FromResult(targetDirectory.Content);
                }
                else
                {
                    throw new FileIsADirectoryException(uri);
                }
            }
            else
            {
                throw new FileNotFoundException(uri);
            }
        }

        public ValueTask Rename(Uri oldUri, Uri newUri, bool overwrite)
        {
            var oldPathParts = SplitPath(GetRealPath(oldUri));
            var newPathParts = SplitPath(GetRealPath(newUri));

            if (TryGetFile(oldPathParts.SkipLast(1), out var oldParent) && oldParent is Directory oldParentDirectory &&
                oldParentDirectory.TryGetValue(oldPathParts[^1], out var target))
            {
                if (TryGetFile(newPathParts.SkipLast(1), out var newParent) && newParent is Directory newParentDirectory)
                {
                    if (overwrite)
                    {
                        newParentDirectory[newPathParts[^1]] = target;
                        oldParentDirectory.Remove(oldPathParts[^1]);
                    }
                    else
                    {
                        if (newParentDirectory.TryAdd(newPathParts[^1], target))
                        {
                            oldParentDirectory.Remove(oldPathParts[^1]);
                        }
                        else
                        {
                            throw new FileExistsException(newUri);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException('/' + string.Join('/', newPathParts.SkipLast(1)));
                }
            }
            else
            {
                throw new FileNotFoundException('/' + string.Join('/', oldPathParts));
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<FileStat> Stat(Uri uri)
        {
            var pathParts = SplitPath(GetRealPath(uri));

            if (TryGetFile(pathParts, out var target))
            {
                return ValueTask.FromResult(
                    new FileStat(target.CreationTime, target.LastWriteTime, target is File file ? file.Size : 0, target.Type));
            }
            else
            {
                throw new FileNotFoundException(uri);
            }
        }

        public ValueTask WriteFile(Uri uri, byte[] content, bool create = true, bool overwrite = true)
        {
            var pathParts = SplitPath(GetRealPath(uri));

            if (TryGetFile(pathParts.SkipLast(1), out var parent) && parent is Directory parentDirectory)
            {
                if (parentDirectory.TryGetValue(pathParts[^1], out var target))
                {
                    if (overwrite == false)
                    {
                        throw new FileExistsException(uri);
                    }

                    if (target is File file)
                    {
                        file.Content = content;
                        file.LastWriteTime = DateTimeOffset.Now;
                    }
                    else
                    {
                        throw new FileIsADirectoryException();
                    }
                }
                else
                {
                    if (create == false)
                    {
                        throw new FileNotFoundException(uri);
                    }

                    parentDirectory.Add(pathParts[^1], new File(content));
                }
            }
            else
            {
                throw new FileNotFoundException('/' + string.Join('/', pathParts.SkipLast(1)));
            }

            return ValueTask.CompletedTask;
        }

        private class Directory : Entity, IEnumerable<KeyValuePair<string, Entity>>
        {
            private Dictionary<string, Entity> Children { get; } = new();

            public Directory()
                : base(FileType.Directory)
            {
            }

            public Entity this[string key]
            {
                get => Children[key];
                set => Children[key] = value;
            }

            public void Add(string key, Entity value)
            {
                Children.Add(key, value);
                UpdateLastWriteTime();
            }

            public bool TryAdd(string key, Entity value)
            {
                if (Children.TryAdd(key, value))
                {
                    UpdateLastWriteTime();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out Entity value)
            {
                return Children.TryGetValue(key, out value);
            }

            public bool Remove(string key)
            {
                if (Children.Remove(key))
                {
                    UpdateLastWriteTime();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override object Clone()
            {
                var directory = new Directory();
                foreach (var pair in Children)
                {
                    var name = pair.Key;
                    var child = (Entity)pair.Value.Clone();
                    directory.Children.Add(name, child);
                }

                return directory;
            }

            public IEnumerator<KeyValuePair<string, Entity>> GetEnumerator()
            {
                return Children.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class File
            : Entity
        {
            private byte[] _content;

            public byte[] Content
            {
                get => _content;
                set
                {
                    _content = value;
                    UpdateLastWriteTime();
                }
            }

            public long Size => Content.Length;

            public File(byte[] content)
                : base(FileType.File)
            {
                _content = content;
            }

            public override object Clone()
            {
                return new File((byte[])Content.Clone());
            }
        }

        private abstract class Entity : ICloneable
        {
            public DateTimeOffset CreationTime { get; }

            public DateTimeOffset LastWriteTime { get; set; }

            public FileType Type { get; }

            protected Entity(FileType type)
            {
                CreationTime = DateTimeOffset.UtcNow;
                LastWriteTime = DateTimeOffset.UtcNow;
                Type = type;
            }

            public abstract object Clone();

            public void UpdateLastWriteTime()
            {
                LastWriteTime = DateTimeOffset.Now;
            }
        }
    }
}
