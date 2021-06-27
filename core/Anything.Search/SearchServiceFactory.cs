using Anything.FileSystem;
using Anything.Search.Crawlers;
using Anything.Search.Indexers;

namespace Anything.Search
{
    public static class SearchServiceFactory
    {
        public static ISearchService BuildSearchService(IFileService fileService, string indexPath)
        {
            var indexer = new LuceneIndexer(indexPath);
            return new SearchService(fileService, indexer, new ISearchCrawler[] { new FileNameSearchCrawler() });
        }
    }
}
