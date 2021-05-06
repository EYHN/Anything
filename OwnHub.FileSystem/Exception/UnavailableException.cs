using OwnHub.Utils;

namespace OwnHub.FileSystem.Exception
{
    public class UnavailableException : FileSystemException
    {
        public UnavailableException()
        {
        }

        public UnavailableException(Url? uri)
            : base(uri)
        {
        }

        public UnavailableException(string? message)
            : base(message)
        {
        }

        public UnavailableException(Url? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
