using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Server.Abstractions.Graphql.Models;

public class DirectoryEntry : FileEntry
{
    public DirectoryEntry(ApplicationEntry applicationEntry, FileHandle fileHandle, FileStats stats)
        : base(applicationEntry, fileHandle, stats)
    {
    }

    public async ValueTask<IEnumerable<DirentEntry>> ReadEntries()
    {
        var entries = await ApplicationEntry.FileService.ReadDirectory(FileHandle.Value);
        return entries.Select(entry => new DirentEntry(entry.Name, ApplicationEntry.CreateFile(entry.FileHandle, entry.Stats)));
    }
}
