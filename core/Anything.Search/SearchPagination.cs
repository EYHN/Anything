namespace Anything.Search
{
    public record SearchPagination(int Size, string? After = null, string? ScrollId = null);
}
