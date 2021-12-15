using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Search.Properties;
using Anything.Utils;

namespace Anything.Search.Indexers;

public interface ISearchIndexer
{
    public ValueTask BatchIndex((Url Url, FileHandle FileHandle, SearchPropertyValueSet Properties)[] payload);

    public ValueTask BatchDelete(FileHandle[] fileHandles);

    public ValueTask<SearchResult> Search(SearchOptions options);

    public ValueTask ForceRefresh();
}
