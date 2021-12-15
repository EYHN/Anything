using System.Collections.Generic;
using System.Threading;
using Anything.FileSystem.Singleton;
using Anything.Utils;

namespace Anything.FileSystem.Walker;

public static class FileSystemWalkerFactory
{
    public static IFileSystemWalker FromEnumerable(
        FileHandle rootFileHandle,
        IAsyncEnumerable<FileSystemWalkerEntry> enumerable)
    {
        return new FileSystemWalkerFromEnumerable(rootFileHandle, enumerable);
    }

    public static IFileSystemWalker CreateGenericWalker(ISingletonFileSystem singletonFileSystem, FileHandle rootFileHandle)
    {
        return FromEnumerable(rootFileHandle, EnumerateAllFiles(singletonFileSystem, rootFileHandle));
    }

    private static async IAsyncEnumerable<FileSystemWalkerEntry> EnumerateAllFiles(
        ISingletonFileSystem singletonFileSystem,
        FileHandle rootFileHandle)
    {
        var directoryWalker = new FileSystemDirectoryWalker(singletonFileSystem, rootFileHandle);
        await foreach (var directory in directoryWalker)
        {
            foreach (var entries in directory.Entries)
            {
                yield return new FileSystemWalkerEntry(entries.FileHandle, entries.Stats, PathLib.Join(directory.Path, entries.Name));
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
