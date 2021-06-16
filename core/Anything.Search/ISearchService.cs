using System.Threading.Tasks;

namespace Anything.Search
{
    public interface ISearchService
    {
        public ValueTask Search(SearchOption searchOption);
    }
}
