using System.Linq;
using Anything.Utils;

namespace Anything.Search
{
    public record SearchResult(Url[] Items)
    {
        public static SearchResult Merge(params SearchResult[] results)
        {
            return new(results.SelectMany(result => result.Items).ToArray());
        }
    }
}
