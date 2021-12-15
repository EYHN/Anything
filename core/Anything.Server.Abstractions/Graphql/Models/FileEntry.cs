using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Server.Abstractions.Graphql.Models;

public abstract class FileEntry
{
    private readonly FileHandle _rawFileHandle;

    protected FileEntry(ApplicationEntry applicationEntry, FileHandle fileHandle, FileStats stats)
    {
        ApplicationEntry = applicationEntry;
        _rawFileHandle = fileHandle;
        Stats = stats;
    }

    protected ApplicationEntry ApplicationEntry { get; }

    public FileHandleRefEntry FileHandle => new(ApplicationEntry, _rawFileHandle);

    public FileStats Stats { get; }

    public ValueTask<Url> GetUrl()
    {
        return ApplicationEntry.FileService.GetUrl(_rawFileHandle);
    }

    public ValueTask<string> GetFileName()
    {
        return ApplicationEntry.FileService.GetFileName(_rawFileHandle);
    }
}
