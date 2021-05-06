

using OwnHub.Utils;

namespace OwnHub.FileSystem.Exception
{
    public class FileExistsException : FileSystemException
    {
        public FileExistsException()
        {
        }

        public FileExistsException(Url? uri)
            : base(uri)
        {
        }

        public FileExistsException(string? message)
            : base(message)
        {
        }

        public FileExistsException(Url? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
