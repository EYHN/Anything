using System.IO;
using System.Reflection;
using System.Threading;
using Anything.Database;
using Anything.Database.Provider;
using Anything.FileSystem.Provider;
using Microsoft.Extensions.FileProviders;

namespace Anything.FileSystem
{
    public static class FileServiceFactory
    {
        private static int _memoryConnectionSequenceId;

        public static IFileService BuildLocalFileService(string rootPath, string cachePath)
        {
            Directory.CreateDirectory(cachePath);

            var fileSystem = new VirtualFileSystem(
                new LocalFileSystemProvider(rootPath),
                new SqliteContext(Path.Join(cachePath, "tracker.db")));
            return new FileService(fileSystem);
        }

        private static SqliteContext BuildSharedMemorySqliteContext()
        {
            var connectionProvider =
                new SharedMemoryConnectionProvider("memory-file-tracker-" + Interlocked.Increment(ref _memoryConnectionSequenceId));
            return new SqliteContext(connectionProvider);
        }

        public static IFileService BuildMemoryFileService()
        {
            var fileSystem = new VirtualFileSystem(new MemoryFileSystemProvider(), BuildSharedMemorySqliteContext());
            return new FileService(fileSystem);
        }

        public static IFileService BuildEmbeddedFileService(Assembly assembly)
        {
            var fileSystem = new VirtualFileSystem(
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(assembly)),
                BuildSharedMemorySqliteContext());
            return new FileService(fileSystem);
        }
    }
}
