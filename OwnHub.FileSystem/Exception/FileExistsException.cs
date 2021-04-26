using System;

namespace OwnHub.FileSystem.Exception
{
    public class FileExistsException : FileSystemException
    {
        public FileExistsException()
        {
        }

        public FileExistsException(Uri? uri)
            : base(uri)
        {
        }

        public FileExistsException(string? message)
            : base(message)
        {
        }

        public FileExistsException(Uri? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
