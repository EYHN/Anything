using System.IO;
using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem.Indexer.Database;

namespace Anything.FileSystem
{
    public static class FileStstemServiceFactory
    {
        public static async ValueTask<IFileSystemService> BuildFileSystemService(string cachePath)
        {
            Directory.CreateDirectory(cachePath);
            var cacheStorage = new DatabaseFileIndexer(new SqliteContext(Path.Join(cachePath, "indexer.db")));
            await cacheStorage.Create();
            var fileSystemService = new VirtualFileSystemService(cacheStorage);
            return fileSystemService;
        }
    }
}
