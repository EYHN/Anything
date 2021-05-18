using StagingBox.Utils;

namespace StagingBox.FileSystem.Exception
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
