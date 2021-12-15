using Anything.FileSystem;

namespace Anything.Server.Abstractions.Graphql.Models;

public class UnknownFileEntry : FileEntry
{
    public UnknownFileEntry(ApplicationEntry applicationEntry, FileHandle fileHandle, FileStats stats)
        : base(applicationEntry, fileHandle, stats)
    {
    }
}
