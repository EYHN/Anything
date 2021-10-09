using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Search.Crawlers;
using Anything.Search.Indexers;
using Anything.Search.Properties;
using Anything.Utils;
using Anything.Utils.Event;

namespace Anything.Search
{
    public class SearchService : Disposable, ISearchService
    {
        private readonly ISearchCrawler[] _crawlers;
        private readonly IFileService _fileService;
        private readonly ISearchIndexer _indexer;
        private EventDisposable? _fileEventListener;

        public SearchService(IFileService fileService, ISearchIndexer indexer, ISearchCrawler[] crawlers)
        {
            _indexer = indexer;
            _crawlers = crawlers;
            _fileService = fileService;

            SetupAutoIndex();
        }

        public ValueTask<SearchResult> Search(SearchOptions searchOptions)
        {
            return _indexer.Search(searchOptions);
        }

        private void SetupAutoIndex()
        {
            _fileEventListener?.Dispose();
            _fileEventListener = _fileService.FileEvent.On(async events =>
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
            });
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

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileEventListener?.Dispose();
        }
    }
}
