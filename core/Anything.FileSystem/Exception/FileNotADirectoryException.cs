using Anything.Utils;

namespace Anything.FileSystem.Exception;

public class FileNotADirectoryException : FileSystemException
{
    public FileNotADirectoryException(Url uri)
        : base(uri)
    {
    }

    public FileNotADirectoryException(FileHandle fileHandle)
        : base(fileHandle)
    {
    }

    public FileNotADirectoryException(string message)
        : base(message)
    {
    }

    public FileNotADirectoryException(Url uri, string message)
        : base(uri, message)
    {
    }

    public FileNotADirectoryException(FileHandle fileHandle, string message)
        : base(fileHandle, message)
    {
    }
}
