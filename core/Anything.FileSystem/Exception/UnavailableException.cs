using Anything.Utils;

namespace Anything.FileSystem.Exception;

public class UnavailableException : FileSystemException
{
    public UnavailableException(Url uri)
        : base(uri)
    {
    }

    public UnavailableException(FileHandle fileHandle)
        : base(fileHandle)
    {
    }

    public UnavailableException(string message)
        : base(message)
    {
    }

    public UnavailableException(Url uri, string message)
        : base(uri, message)
    {
    }

    public UnavailableException(FileHandle fileHandle, string message)
        : base(fileHandle, message)
    {
    }
}
