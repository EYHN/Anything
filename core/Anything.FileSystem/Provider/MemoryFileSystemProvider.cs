using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem.Exception;
using Anything.Utils;
using FileNotFoundException = Anything.FileSystem.Exception.FileNotFoundException;

namespace Anything.FileSystem.Provider
{
    /// <summary>
    ///     File system provider, store files in memory.
    /// </summary>
    public class MemoryFileSystemProvider
        : IFileSystemProviderSupportStream
    {
        private readonly Directory _rootDirectory = new();

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

        public ValueTask<IEnumerable<(string Name, FileStats Stats)>> ReadDirectory(Url url)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts, out var target))
            {
                if (target is Directory targetDirectory)
                {
                    return ValueTask.FromResult(
                        targetDirectory.Select(pair => (pair.Key, pair.Value.Stats)));
                }

                throw new FileNotADirectoryException(url);
            }

            throw new FileNotFoundException(url);
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

                throw new FileIsADirectoryException(url);
            }

            throw new FileNotFoundException(url);
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

        public ValueTask<FileStats> Stat(Url url)
        {
            var pathParts = SplitPath(GetRealPath(url));

            if (TryGetFile(pathParts, out var target))
            {
                return ValueTask.FromResult(
                    target.Stats);
            }

            throw new FileNotFoundException(url);
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

        public async ValueTask<Stream> OpenReadFileStream(Url url)
        {
            var data = await ReadFile(url);
            return new MemoryStream(data, false);
        }

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

        private class Directory : Entity, IEnumerable<KeyValuePair<string, Entity>>
        {
            public Directory()
                : base(FileType.Directory)
            {
            }

            private Dictionary<string, Entity> Children { get; } = new();

            public Entity this[string key]
            {
                get => Children[key];
                set => Children[key] = value;
            }

            public IEnumerator<KeyValuePair<string, Entity>> GetEnumerator()
            {
                return Children.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
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

                return false;
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

                return false;
            }
        }

        private class File
            : Entity
        {
            private byte[] _content;

            public File(byte[] content)
                : base(FileType.File)
            {
                _content = content;
            }

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
        }

        private abstract class Entity
        {
            protected Entity(FileType type)
            {
                CreationTime = DateTimeOffset.UtcNow;
                LastWriteTime = DateTimeOffset.UtcNow;
                Type = type;
            }

            public DateTimeOffset CreationTime { get; }

            public DateTimeOffset LastWriteTime { get; set; }

            public FileType Type { get; }

            public FileStats Stats => new(CreationTime, LastWriteTime, this is File file ? file.Size : 0, Type);

            public void UpdateLastWriteTime()
            {
                LastWriteTime = DateTimeOffset.Now;
            }
        }
    }
}
