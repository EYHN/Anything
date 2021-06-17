using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anything.Search
{
    public class SearchService : ISearchService
    {
        private readonly ISearchModule[] _modules;

        public SearchService(params ISearchModule[] modules)
        {
            _modules = modules;
        }

        public async Task<SearchResult> Search(SearchOption searchOption)
        {
            var results = new List<SearchResult>();
            foreach (var module in _modules)
            {
                results.Add(await module.Search(searchOption));
            }

            return SearchResult.Merge(results.ToArray());
        }
    }
}
