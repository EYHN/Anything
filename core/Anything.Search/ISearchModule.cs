using System.Threading.Tasks;

namespace Anything.Search
{
    public interface ISearchModule
    {
        public Task<SearchResult> Search(SearchOptions optionses);
    }
}
