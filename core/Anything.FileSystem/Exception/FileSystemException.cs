using Anything.Utils;

namespace Anything.FileSystem.Exception;

public class FileSystemException
    : System.Exception
{
    public FileSystemException(Url uri)
    {
        Uri = uri;
    }

    public FileSystemException(FileHandle fileHandle)
    {
        FileHandle = fileHandle;
    }

    public FileSystemException(string message)
        : base(message)
    {
    }

    public FileSystemException(Url uri, string message)
        : base(message)
    {
        Uri = uri;
    }

    public FileSystemException(FileHandle fileHandle, string message)
        : base(message)
    {
        FileHandle = fileHandle;
    }

    public Url? Uri { get; }

    public FileHandle? FileHandle { get; }
}
