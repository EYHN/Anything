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
                return Task.FromResult(new SearchPropertyValueSet(new[] { (SearchProperty.FileName, url.Basename() as object) }));
            }

            return Task.FromResult(new SearchPropertyValueSet(Array.Empty<(SearchProperty Property, object Data)>()));
        }
    }
}
