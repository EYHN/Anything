using Anything.Search.Query;
using Anything.Utils;

namespace Anything.Search
{
    public record SearchOptions
    {
        public static readonly SearchPagination DefaultPagination = new(10, null, null);

        public SearchOptions(SearchQuery query, Url? baseUrl = null, SearchPagination? pagination = null)
        {
            Query = query;
            BaseUrl = baseUrl;
            Pagination = pagination ?? DefaultPagination;
        }

        public SearchQuery Query { get; }

        public Url? BaseUrl { get; }

        public SearchPagination Pagination { get; }
    }
}
