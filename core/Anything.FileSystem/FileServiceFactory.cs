using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem.Provider;
using Anything.FileSystem.Tracker.Database;
using Anything.FileSystem.Tracker.Memory;
using Microsoft.Extensions.FileProviders;

namespace Anything.FileSystem
{
    public static class FileServiceFactory
    {
        public static async ValueTask<IFileService> BuildLocalFileService(string rootPath, string cachePath)
        {
            Directory.CreateDirectory(cachePath);

            var fileSystem = new VirtualFileSystem(new LocalFileSystemProvider(rootPath));
            var fileTracker = new DatabaseFileTracker(fileSystem, new SqliteContext(Path.Join(cachePath, "tracker.db")));
            await fileTracker.Create();
            return new FileService(fileSystem, fileTracker);
        }

        public static async ValueTask<IFileService> BuildMemoryFileService()
        {
            var fileSystem = new VirtualFileSystem(new MemoryFileSystemProvider());
            var fileTracker = new MemoryFileTracker(fileSystem);
            await fileTracker.Create();
            return new FileService(fileSystem, fileTracker);
        }

        public static async ValueTask<IFileService> BuildEmbeddedFileService(Assembly assembly)
        {
            var fileSystem = new VirtualFileSystem(new EmbeddedFileSystemProvider(new EmbeddedFileProvider(assembly)));
            var fileTracker = new MemoryFileTracker(fileSystem);
            await fileTracker.Create();
            return new FileService(fileSystem, fileTracker);
        }
    }
}
