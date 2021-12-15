using Anything.FileSystem;

namespace Anything.Server.Abstractions.Graphql.Models;

public class RegularFileEntry : FileEntry
{
    public RegularFileEntry(ApplicationEntry applicationEntry, FileHandle fileHandle, FileStats stats)
        : base(applicationEntry, fileHandle, stats)
    {
    }
}
