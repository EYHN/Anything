using System.IO;
using Anything.Database;
using Anything.FileSystem;
using Anything.Search.Modules.FileName;

namespace Anything.Search
{
    public class SearchServiceFactory
    {
        public static ISearchService BuildSearchService(IFileService fileService, string cachePath)
        {
            Directory.CreateDirectory(Path.Join(cachePath, "index"));
            var filenameModule = new FileNameSearch(new SqliteContext(Path.Join(cachePath, "index", "index.db")), fileService);
            return new SearchService(filenameModule);
        }
    }
}
