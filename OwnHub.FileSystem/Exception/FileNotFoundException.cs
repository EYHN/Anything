using System;

namespace OwnHub.FileSystem.Exception
{
    public class FileNotFoundException : FileSystemException
    {
        public FileNotFoundException()
        {
        }

        public FileNotFoundException(Uri? uri)
            : base(uri)
        {
        }

        public FileNotFoundException(string? message)
            : base(message)
        {
        }

        public FileNotFoundException(Uri? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
