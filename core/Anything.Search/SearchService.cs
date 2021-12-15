using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Search.Crawlers;
using Anything.Search.Indexers;
using Anything.Search.Properties;
using Anything.Utils;

namespace Anything.Search;

public class SearchService : ISearchService
{
    private readonly ISearchIndexer _indexer;

    public SearchService(ISearchIndexer indexer)
    {
        _indexer = indexer;
    }

    public ValueTask<SearchResult> Search(SearchOptions searchOptions)
    {
        return _indexer.Search(searchOptions);
    }

    public class FileEventHandler : IFileEventHandler
    {
        private readonly ISearchCrawler[] _crawlers;
        private readonly IFileService _fileService;
        private readonly ISearchIndexer _indexer;

        public FileEventHandler(IFileService fileService, ISearchIndexer indexer, IEnumerable<ISearchCrawler> crawlers)
        {
            _fileService = fileService;
            _indexer = indexer;
            _crawlers = crawlers.ToArray();
        }

        public async ValueTask OnFileEvent(IEnumerable<FileEvent> events)
        {
            var indexList = new List<FileHandle>();
            var deleteList = new List<FileHandle>();
            foreach (var @event in events)
            {
                if (@event.Type is FileEvent.EventType.Created)
                {
                    indexList.Add(@event.FileHandle);
                }

                if (@event.Type is FileEvent.EventType.Deleted)
                {
                    deleteList.Add(@event.FileHandle);
                }
            }

            if (deleteList.Count > 0)
            {
                await BatchDelete(deleteList.ToArray());
            }

            if (indexList.Count > 0)
            {
                await BatchIndex(indexList.ToArray());
            }
        }

        private async ValueTask BatchIndex(FileHandle[] fileHandles)
        {
            var files = new List<(Url Url, FileHandle FileHandle, SearchPropertyValueSet Properties)>();
            foreach (var fileHandle in fileHandles)
            {
                var url = await _fileService.GetUrl(fileHandle);
                var propertyValueSets = await Task.WhenAll(_crawlers.Select(c => c.GetData(fileHandle).AsTask()));
                var properties = SearchPropertyValueSet.Merge(propertyValueSets);

                files.Add((url, fileHandle, properties));
            }

            await _indexer.BatchIndex(files.ToArray());
        }

        private ValueTask BatchDelete(FileHandle[] fileHandles)
        {
            return _indexer.BatchDelete(fileHandles);
        }
    }
}
