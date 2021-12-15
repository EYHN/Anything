using Anything.Utils;

namespace Anything.FileSystem.Exception;

public class NotSupportedException : FileSystemException
{
    public NotSupportedException(Url uri)
        : base(uri)
    {
    }

    public NotSupportedException(FileHandle fileHandle)
        : base(fileHandle)
    {
    }

    public NotSupportedException(string message)
        : base(message)
    {
    }

    public NotSupportedException(Url uri, string message)
        : base(uri, message)
    {
    }

    public NotSupportedException(FileHandle fileHandle, string message)
        : base(fileHandle, message)
    {
    }
}
