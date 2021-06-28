using System;
using System.Threading.Tasks;
using Anything.Search.Properties;
using Anything.Utils;

namespace Anything.Search.Crawlers
{
    public class FileNameSearchCrawler : ISearchCrawler
    {
        public Task<SearchPropertyValueSet> GetData(Url url)
        {
            var filename = url.Basename();
            if (filename != string.Empty)
            {
                return Task.FromResult(
                    new SearchPropertyValueSet(new[] { new SearchPropertyValue(SearchProperty.FileName, url.Basename()) }));
            }

            return Task.FromResult(new SearchPropertyValueSet(Array.Empty<SearchPropertyValue>()));
        }
    }
}
