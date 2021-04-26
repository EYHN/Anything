using System;

namespace OwnHub.FileSystem.Exception
{
    public class NoPermissionsException : FileSystemException
    {
        public NoPermissionsException()
        {
        }

        public NoPermissionsException(Uri? uri)
            : base(uri)
        {
        }

        public NoPermissionsException(string? message)
            : base(message)
        {
        }

        public NoPermissionsException(Uri? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
