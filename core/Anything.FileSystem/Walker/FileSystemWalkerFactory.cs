using System.Collections.Generic;
using System.Threading;
using Anything.Utils;

namespace Anything.FileSystem.Walker
{
    public static class FileSystemWalkerFactory
    {
        public static IFileSystemWalker FromEnumerable(
            FileHandle rootFileHandle,
            IAsyncEnumerable<FileSystemWalkerEntry> enumerable)
        {
            return new FileSystemWalkerFromEnumerable(rootFileHandle, enumerable);
        }

        public static IFileSystemWalker CreateGenericWalker(IFileSystem fileSystem, FileHandle rootFileHandle)
        {
            return FromEnumerable(rootFileHandle, EnumerateAllFiles(fileSystem, rootFileHandle));
        }

        private static async IAsyncEnumerable<FileSystemWalkerEntry> EnumerateAllFiles(
            IFileSystem fileSystem,
            FileHandle rootFileHandle)
        {
            var directoryWalker = new FileSystemDirectoryWalker(fileSystem, rootFileHandle);
            await foreach (var directory in directoryWalker)
            {
                foreach (var entries in directory.Entries)
                {
                    yield return new(entries.FileHandle, entries.Stats, PathLib.Join(directory.Path, entries.Name));
                }
            }
        }

        private class FileSystemWalkerFromEnumerable : IFileSystemWalker
        {
            private readonly IAsyncEnumerable<FileSystemWalkerEntry> _enumerable;

            public FileSystemWalkerFromEnumerable(
                FileHandle rootFileHandle,
                IAsyncEnumerable<FileSystemWalkerEntry> enumerable)
            {
                _enumerable = enumerable;
                RootFileHandle = rootFileHandle;
            }

            /// <inheritdoc />
            public FileHandle RootFileHandle { get; }

            /// <inheritdoc />
            public IAsyncEnumerator<FileSystemWalkerEntry> GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                return _enumerable.GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}
