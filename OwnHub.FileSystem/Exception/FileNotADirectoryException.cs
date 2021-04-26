using System;

namespace OwnHub.FileSystem.Exception
{
    public class FileNotADirectoryException : FileSystemException
    {
        public FileNotADirectoryException()
        {
        }

        public FileNotADirectoryException(Uri? uri)
            : base(uri)
        {
        }

        public FileNotADirectoryException(string? message)
            : base(message)
        {
        }

        public FileNotADirectoryException(Uri? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
