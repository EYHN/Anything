using OwnHub.Utils;

namespace OwnHub.FileSystem.Exception
{
    public class NoPermissionsException : FileSystemException
    {
        public NoPermissionsException()
        {
        }

        public NoPermissionsException(Url? uri)
            : base(uri)
        {
        }

        public NoPermissionsException(string? message)
            : base(message)
        {
        }

        public NoPermissionsException(Url? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
