using OwnHub.Utils;

namespace OwnHub.FileSystem.Exception
{
    public class FileSystemException
        : System.Exception
    {
        public Url? Uri { get; }

        public FileSystemException()
        {
        }

        public FileSystemException(Url? uri)
        {
            Uri = uri;
        }

        public FileSystemException(string? message)
            : base(message)
        {
        }

        public FileSystemException(Url? uri, string? message)
            : base(message)
        {
            Uri = uri;
        }
    }
}
