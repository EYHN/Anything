using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Search.Properties;

namespace Anything.Search.Crawlers;

public interface ISearchCrawler
{
    public ValueTask<SearchPropertyValueSet> GetData(FileHandle fileHandle);
}
