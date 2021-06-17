using System.Threading.Tasks;

namespace Anything.Search
{
    public interface ISearchService
    {
        public Task<SearchResult> Search(SearchOption searchOption);
    }
}
