using Anything.Utils;

namespace Anything.FileSystem.Exception
{
    public class FileSystemException
        : System.Exception
    {
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

        public Url? Uri { get; }
    }
}
