using System.Threading.Tasks;
using Anything.Database;
using Anything.FileSystem;

namespace Anything.Search.Modules.FileName
{
    public class FileNameSearch : ISearchModule
    {
        private readonly FileNameIndexer _fileNameIndexer;

        public FileNameSearch(SqliteContext sqliteContext, IFileService fileService)
        {
            _fileNameIndexer = new FileNameIndexer(sqliteContext);
            _fileNameIndexer.BindingAutoIndex(fileService);
        }

        public async Task<SearchResult> Search(SearchOption options)
        {
            var urls = await _fileNameIndexer.Search(options.Q, options.BaseUrl);
            return new SearchResult(urls);
        }
    }
}
