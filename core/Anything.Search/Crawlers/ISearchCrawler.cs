using System.Threading.Tasks;
using Anything.Search.Properties;
using Anything.Utils;

namespace Anything.Search.Crawlers
{
    public interface ISearchCrawler
    {
        public Task<SearchPropertyValueSet> GetData(Url url);
    }
}
