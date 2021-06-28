using Anything.Utils;

namespace Anything.Search
{
    public record SearchResult(SearchResultNode[] Nodes, SearchPageInfo PageInfo);

    public record SearchResultNode(Url Url);

    public record SearchPageInfo(int TotalCount);
}
