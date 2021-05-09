using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;
using OwnHub.Utils;

namespace OwnHub.FileSystem.Provider
{
    public class MemoryFileSystemProvider
        : IFileSystemProvider
    {
        private Directory _rootDirectory = new();

        private static string GetRealPath(Url url)
        {
            return PathLib.Resolve(url.Path);
        }

        private static string[] SplitPath(string path)
        {
            return PathLib.Split(path);
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

        public ValueTask CreateDirectory(Url url)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts.SkipLast(1), out var parent) && parent is Directory parentDirectory)
            {
                if (parentDirectory.TryAdd(pathParts[^1], new Directory()) == false)
                {
                    throw new FileExistsException(url);
                }
            }
            else
            {
                throw new FileNotFoundException(url.Dirname());
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask Delete(Url url, bool recursive)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts.SkipLast(1), out var parent) && parent is Directory parentDirectory &&
                parentDirectory.TryGetValue(pathParts[^1], out var target))
            {
                if (recursive == false && target is Directory)
                {
                    throw new FileIsADirectoryException(url);
                }

                parentDirectory.Remove(pathParts[^1]);
            }
            else
            {
                throw new FileNotFoundException(url);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<IEnumerable<KeyValuePair<string, FileType>>> ReadDirectory(Url url)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts, out var target))
            {
                if (target is Directory targetDirectory)
                {
                    return ValueTask.FromResult(
                        targetDirectory.Select(pair => new KeyValuePair<string, FileType>(pair.Key, pair.Value.Type)));
                }
                else
                {
                    throw new FileNotADirectoryException(url);
                }
            }
            else
            {
                throw new FileNotFoundException(url);
            }
        }

        public ValueTask<byte[]> ReadFile(Url url)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts, out var target))
            {
                if (target is File targetDirectory)
                {
                    return ValueTask.FromResult(targetDirectory.Content);
                }
                else
                {
                    throw new FileIsADirectoryException(url);
                }
            }
            else
            {
                throw new FileNotFoundException(url);
            }
        }

        public ValueTask Rename(Url oldUrl, Url newUrl, bool overwrite)
        {
            var oldPathParts = SplitPath(GetRealPath(oldUrl));
            var newPathParts = SplitPath(GetRealPath(newUrl));

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
                            throw new FileExistsException(newUrl);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException(newUrl.Dirname());
                }
            }
            else
            {
                throw new FileNotFoundException(oldUrl);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<FileStat> Stat(Url url)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts, out var target))
            {
                return ValueTask.FromResult(
                    new FileStat(target.CreationTime, target.LastWriteTime, target is File file ? file.Size : 0, target.Type));
            }
            else
            {
                throw new FileNotFoundException(url);
            }
        }

        public ValueTask WriteFile(Url url, byte[] content, bool create = true, bool overwrite = true)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts.SkipLast(1), out var parent) && parent is Directory parentDirectory)
            {
                if (parentDirectory.TryGetValue(pathParts[^1], out var target))
                {
                    if (overwrite == false)
                    {
                        throw new FileExistsException(url);
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
                        throw new FileNotFoundException(url);
                    }

                    parentDirectory.Add(pathParts[^1], new File(content));
                }
            }
            else
            {
                throw new FileNotFoundException(url.Dirname());
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
        }

        private abstract class Entity
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

            public void UpdateLastWriteTime()
            {
                LastWriteTime = DateTimeOffset.Now;
            }
        }
    }
}
