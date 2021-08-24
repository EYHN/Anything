using Anything.FileSystem;
using Anything.Search.Crawlers;
using Anything.Search.Indexers;

namespace Anything.Search
{
    public static class SearchServiceFactory
    {
        public static ISearchService BuildSearchService(IFileService fileService, ISearchIndexer indexer)
        {
            return new SearchService(fileService, indexer, new ISearchCrawler[] { new FileNameSearchCrawler() });
        }
    }
}
