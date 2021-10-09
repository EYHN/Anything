using Anything.Utils;

namespace Anything.FileSystem.Exception
{
    public class FileExistsException : FileSystemException
    {
        public FileExistsException(Url uri)
            : base(uri)
        {
        }

        public FileExistsException(FileHandle fileHandle)
            : base(fileHandle)
        {
        }

        public FileExistsException(string message)
            : base(message)
        {
        }

        public FileExistsException(Url uri, string message)
            : base(uri, message)
        {
        }

        public FileExistsException(FileHandle fileHandle, string message)
            : base(fileHandle, message)
        {
        }
    }
}
