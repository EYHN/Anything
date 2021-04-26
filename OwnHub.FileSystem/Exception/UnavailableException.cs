using System;

namespace OwnHub.FileSystem.Exception
{
    public class UnavailableException : FileSystemException
    {
        public UnavailableException()
        {
        }

        public UnavailableException(Uri? uri)
            : base(uri)
        {
        }

        public UnavailableException(string? message)
            : base(message)
        {
        }

        public UnavailableException(Uri? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
