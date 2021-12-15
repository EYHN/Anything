using System.Collections.Generic;
using Anything.FileSystem;

namespace Anything.Search;

public record SearchResult(IReadOnlyList<SearchResultNode> Nodes, SearchPageInfo PageInfo);

public record SearchResultNode(FileHandle FileHandle, string Cursor);

public record SearchPageInfo(int TotalCount, string? ScrollId);
