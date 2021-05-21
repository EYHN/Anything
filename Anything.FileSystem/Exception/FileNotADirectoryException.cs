using Anything.Utils;

namespace Anything.FileSystem.Exception
{
    public class FileNotADirectoryException : FileSystemException
    {
        public FileNotADirectoryException()
        {
        }

        public FileNotADirectoryException(Url? uri)
            : base(uri)
        {
        }

        public FileNotADirectoryException(string? message)
            : base(message)
        {
        }

        public FileNotADirectoryException(Url? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
