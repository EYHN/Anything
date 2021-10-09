using Anything.Utils;

namespace Anything.FileSystem.Exception
{
    public class FileIsADirectoryException : FileSystemException
    {
        public FileIsADirectoryException(Url uri)
            : base(uri)
        {
        }

        public FileIsADirectoryException(FileHandle fileHandle)
            : base(fileHandle)
        {
        }

        public FileIsADirectoryException(string message)
            : base(message)
        {
        }

        public FileIsADirectoryException(Url uri, string message)
            : base(uri, message)
        {
        }

        public FileIsADirectoryException(FileHandle fileHandle, string message)
            : base(fileHandle, message)
        {
        }
    }
}
