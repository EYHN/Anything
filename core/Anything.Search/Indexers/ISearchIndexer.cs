using System.Threading.Tasks;
using Anything.Search.Properties;
using Anything.Utils;

namespace Anything.Search.Indexers
{
    public interface ISearchIndexer
    {
        public Task BatchIndex((Url Url, SearchPropertyValueSet Properties)[] payload);

        public Task BatchDelete(Url[] urls);

        public Task<SearchResult> Search(SearchOptions options);

        public Task ForceRefresh();
    }
}
