using System;

namespace OwnHub.FileSystem.Exception
{
    public class FileIsADirectoryException : FileSystemException
    {
        public FileIsADirectoryException()
        {
        }

        public FileIsADirectoryException(Uri? uri)
            : base(uri)
        {
        }

        public FileIsADirectoryException(string? message)
            : base(message)
        {
        }

        public FileIsADirectoryException(Uri? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
