using Anything.Utils;

namespace Anything.Search
{
    public record SearchPagination(int First, Url? After = null);
}
