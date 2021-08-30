using Anything.FileSystem;
using Anything.Search.Crawlers;
using Anything.Search.Indexers;

namespace Anything.Search
{
    public static class SearchServiceFactory
    {
        public static SearchService BuildSearchService(IFileService fileService, ISearchIndexer indexer)
        {
            return new(fileService, indexer, new ISearchCrawler[] { new FileNameSearchCrawler() });
        }
    }
}
