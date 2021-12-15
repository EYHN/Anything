using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Server.Abstractions.Graphql.Models;

public class ApplicationEntry
{
    public ApplicationEntry(IFileService fileService)
    {
        FileService = fileService;
    }

    public IFileService FileService { get; }

    public async ValueTask<FileHandleRefEntry> CreateFileHandle(Url url)
    {
        var fileHandle = await FileService.CreateFileHandle(url);
        return new FileHandleRefEntry(this, fileHandle);
    }

    public ValueTask<FileHandleRefEntry> OpenFileHandle(FileHandle fileHandle)
    {
        return ValueTask.FromResult(new FileHandleRefEntry(this, fileHandle));
    }

    public async ValueTask<FileEntry> CreateFile(FileHandle fileHandle)
    {
        var stats = await FileService.Stat(fileHandle);
        return CreateFile(fileHandle, stats);
    }

    public FileEntry CreateFile(FileHandle fileHandle, FileStats stats)
    {
        if (stats.Type.HasFlag(FileType.File))
        {
            return new RegularFileEntry(this, fileHandle, stats);
        }

        if (stats.Type.HasFlag(FileType.Directory))
        {
            return new DirectoryEntry(this, fileHandle, stats);
        }

        return new UnknownFileEntry(this, fileHandle, stats);
    }
}
