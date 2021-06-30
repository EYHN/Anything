using Anything.Utils;

namespace Anything.Search
{
    public record SearchResult(SearchResultNode[] Nodes, SearchPageInfo PageInfo);

    public record SearchResultNode(Url Url, string Cursor);

    public record SearchPageInfo(int TotalCount, string? ScrollId = null);
}
