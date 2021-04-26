using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.FileSystem.Exception;
using FileNotFoundException = OwnHub.FileSystem.Exception.FileNotFoundException;

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

        public ValueTask Copy(Uri source, Uri destination, bool overwrite)
        {
            var sourceEntity = _rootDirectory.GetEntity(source);
            _rootDirectory = _rootDirectory.SetEntity(destination, sourceEntity, overwrite: overwrite);
            return ValueTask.CompletedTask;
        }

        public ValueTask CreateDirectory(Uri uri)
        {
            var newRootDirectory = _rootDirectory;
            var realPath = GetRealPath(uri);
            var pathParts = PathUtils.Split(realPath);

            var currentPath = "/";

            foreach (var path in pathParts)
            {
                currentPath = PathUtils.Join(currentPath, path);
                newRootDirectory = newRootDirectory.SetEntity(currentPath, new Directory(), false, skipIfExists: true);
            }

            _rootDirectory = newRootDirectory;

            return ValueTask.CompletedTask;
        }

        public ValueTask Delete(Uri uri, bool recursive)
        {
            _rootDirectory = _rootDirectory.SetEntity(uri, null, true, recursiveOverwrite: recursive);
            return ValueTask.CompletedTask;
        }

        public ValueTask<IEnumerable<KeyValuePair<string, FileType>>> ReadDirectory(Uri uri)
        {
            var entity = _rootDirectory.GetEntity(uri);

            if (entity is Directory directory)
            {
                return ValueTask.FromResult(
                    directory.Children.Select(pair => new KeyValuePair<string, FileType>(pair.Key, pair.Value.Type)));
            }

            throw new FileNotADirectoryException();
        }

        public ValueTask<byte[]> ReadFile(Uri uri)
        {
            var entity = _rootDirectory.GetEntity(uri);

            if (entity is File file)
            {
                return ValueTask.FromResult(file.Content);
            }

            throw new FileIsADirectoryException();
        }

        public async ValueTask Rename(Uri oldUri, Uri newUri, bool overwrite)
        {
            await Copy(oldUri, newUri, overwrite);
            await Delete(oldUri, true);
        }

        public ValueTask<FileStat> Stat(Uri uri)
        {
            var entity = _rootDirectory.GetEntity(uri);

            return ValueTask.FromResult(
                new FileStat(
                    entity.CreationTime,
                    entity.LastWriteTime,
                    entity is File file ? file.Size : 0,
                    entity.Type));
        }

        public ValueTask WriteFile(Uri uri, byte[] content, bool create, bool overwrite)
        {
            File newFile;
            try
            {
                var oldEntity = _rootDirectory.GetEntity(uri);
                if (oldEntity is File oldFile)
                {
                    newFile = oldFile with { Content = content.ToArray(), LastWriteTime = DateTimeOffset.UtcNow };
                }
                else
                {
                    throw new FileIsADirectoryException();
                }
            }
            catch (FileNotFoundException)
            {
                newFile = new File(content.ToArray());
            }

            _rootDirectory = _rootDirectory.SetEntity(
                uri,
                newFile,
                overwrite: true,
                changeTime: false);

            return ValueTask.CompletedTask;
        }

        private record Directory : Entity
        {
            public ImmutableDictionary<string, Entity> Children { get; init; } = ImmutableDictionary.Create<string, Entity>();

            public Directory()
                : base(FileType.Directory)
            {
            }

            public Entity GetEntity(Uri uri)
            {
                return GetEntity(GetRealPath(uri));
            }

            public Entity GetEntity(string path)
            {
                return GetEntity(PathUtils.Split(path).ToImmutableArray());
            }

            public Entity GetEntity(ImmutableArray<string> pathParts)
            {
                var currentDirectory = this;
                for (var i = 0; i < pathParts.Length - 1; i++)
                {
                    var part = pathParts[i];
                    if (currentDirectory.Children.TryGetValue(part, out var newDirectory))
                    {
                        if (newDirectory is File)
                        {
                            throw new FileNotFoundException("/" + string.Join('/', pathParts) + " is file.");
                        }

                        currentDirectory = (Directory)newDirectory;
                    }
                    else
                    {
                        throw new FileNotFoundException("/" + string.Join('/', pathParts) + " not found.");
                    }
                }

                if (pathParts.Length == 0)
                {
                    return currentDirectory;
                }

                if (currentDirectory.Children.TryGetValue(pathParts[^1], out var entity))
                {
                    return entity;
                }

                throw new FileNotFoundException("/" + string.Join('/', pathParts) + " not found.");
            }

            public Directory SetEntity(
                Uri uri,
                Entity? entity,
                bool overwrite,
                bool recursiveOverwrite = false,
                bool skipIfExists = false,
                bool changeTime = true)
            {
                return SetEntity(
                    GetRealPath(uri),
                    entity,
                    overwrite: overwrite,
                    recursiveOverwrite: recursiveOverwrite,
                    skipIfExists: skipIfExists,
                    changeTime: changeTime);
            }

            public Directory SetEntity(
                string path,
                Entity? entity,
                bool overwrite,
                bool recursiveOverwrite = false,
                bool skipIfExists = false,
                bool changeTime = true)
            {
                return SetEntity(
                    PathUtils.Split(path).ToImmutableArray(),
                    entity,
                    overwrite: overwrite,
                    recursiveOverwrite: recursiveOverwrite,
                    skipIfExists: skipIfExists,
                    changeTime: changeTime);
            }

            public Directory SetEntity(
                ImmutableArray<string> pathParts,
                Entity? entity,
                bool overwrite,
                bool recursiveOverwrite = false,
                bool skipIfExists = false,
                bool changeTime = true)
            {
                var parentPathParts = pathParts.RemoveAt(pathParts.Length - 1);
                var parentEntity = GetEntity(parentPathParts);

                if (parentEntity is Directory directory)
                {
                    if (directory.Children.TryGetValue(pathParts[^1], out var oldValue))
                    {
                        if (skipIfExists)
                        {
                            return this;
                        }

                        if (!overwrite)
                        {
                            throw new FileExistsException("/" + string.Join('/', pathParts));
                        }

                        if (!recursiveOverwrite && oldValue is Directory oldDirectory && oldDirectory.Children.Count > 0)
                        {
                            throw new FileIsADirectoryException("/" + string.Join('/', pathParts));
                        }
                    }

                    var newDirectory = directory with
                    {
                        Children = entity != null
                            ? directory.Children.SetItem(pathParts[^1], entity)
                            : directory.Children.Remove(pathParts[^1]),
                        LastWriteTime = changeTime ? DateTimeOffset.UtcNow : directory.LastWriteTime
                    };

                    return directory == this
                        ? newDirectory
                        : SetEntity(
                            parentPathParts,
                            newDirectory,
                            overwrite: true,
                            recursiveOverwrite: true,
                            skipIfExists: false,
                            changeTime: false);
                }
                else
                {
                    throw new FileNotFoundException("/" + string.Join('/', pathParts));
                }
            }
        }

        private record File
            : Entity
        {
            public byte[] Content { get; init; }

            public long Size => Content.Length;

            public File(byte[] content)
                : base(FileType.File)
            {
                Content = content;
            }
        }

        private abstract record Entity
        {
            public DateTimeOffset CreationTime { get; }

            public DateTimeOffset LastWriteTime { get; init; }

            public FileType Type { get; }

            protected Entity(FileType type)
            {
                CreationTime = DateTimeOffset.UtcNow;
                LastWriteTime = DateTimeOffset.UtcNow;
                Type = type;
            }
        }
    }
}
