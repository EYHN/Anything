using System;

namespace OwnHub.FileSystem.Exception
{
    public class FileSystemException
        : System.Exception
    {
        public Uri? Uri { get; }

        public FileSystemException()
        {
        }

        public FileSystemException(Uri? uri)
        {
            Uri = uri;
        }

        public FileSystemException(string? message)
            : base(message)
        {
        }

        public FileSystemException(Uri? uri, string? message)
            : base(message)
        {
            Uri = uri;
        }
    }
}
