using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Search
{
    public record SearchResult(SearchResultNode[] Nodes, SearchPageInfo PageInfo);

    public record SearchResultNode(FileHandle FileHandle, string Cursor);

    public record SearchPageInfo(int TotalCount, string? ScrollId = null);
}
