using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Exception;

namespace Anything.Server.Abstractions.Graphql.Models;

public record FileHandleRefEntry(ApplicationEntry ApplicationEntry, FileHandle Value)
{
    public async ValueTask<DirectoryEntry> OpenDirectory()
    {
        var file = await OpenFile();

        if (!(file is DirectoryEntry directory))
        {
            throw new FileNotADirectoryException(Value);
        }

        return directory;
    }

    public async ValueTask<FileEntry> OpenFile()
    {
        return await ApplicationEntry.CreateFile(Value);
    }
}
