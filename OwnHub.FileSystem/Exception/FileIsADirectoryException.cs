using OwnHub.Utils;

namespace OwnHub.FileSystem.Exception
{
    public class FileIsADirectoryException : FileSystemException
    {
        public FileIsADirectoryException()
        {
        }

        public FileIsADirectoryException(Url? uri)
            : base(uri)
        {
        }

        public FileIsADirectoryException(string? message)
            : base(message)
        {
        }

        public FileIsADirectoryException(Url? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
