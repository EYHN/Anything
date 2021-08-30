using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
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

        public Task<SearchResult> Search(SearchOptions searchOptions)
        {
            return _indexer.Search(searchOptions);
        }

        private void SetupAutoIndex()
        {
            _fileEventListener?.Dispose();
            _fileEventListener = _fileService.FileEvent.On(async events =>
            {
                var indexList = new List<Url>();
                var deleteList = new List<Url>();
                foreach (var @event in events)
                {
                    if (@event.Type is FileEvent.EventType.Created)
                    {
                        indexList.Add(@event.Url);
                    }

                    if (@event.Type is FileEvent.EventType.Deleted)
                    {
                        deleteList.Add(@event.Url);
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

        private async Task BatchIndex(Url[] urls)
        {
            var files = new List<(Url Url, SearchPropertyValueSet Properties)>();
            foreach (var url in urls)
            {
                var propertyValueSets = await Task.WhenAll(_crawlers.Select(c => c.GetData(url)));
                var properties = SearchPropertyValueSet.Merge(propertyValueSets);

                files.Add((url, properties));
            }

            await _indexer.BatchIndex(files.ToArray());
        }

        private Task BatchDelete(Url[] urls)
        {
            return _indexer.BatchDelete(urls);
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileEventListener?.Dispose();
        }
    }
}
