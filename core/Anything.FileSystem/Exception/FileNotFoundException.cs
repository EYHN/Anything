using Anything.Utils;

namespace Anything.FileSystem.Exception
{
    public class FileNotFoundException : FileSystemException
    {
        public FileNotFoundException(Url uri)
            : base(uri)
        {
        }

        public FileNotFoundException(FileHandle fileHandle)
            : base(fileHandle)
        {
        }

        public FileNotFoundException(string message)
            : base(message)
        {
        }

        public FileNotFoundException(Url uri, string message)
            : base(uri, message)
        {
        }

        public FileNotFoundException(FileHandle fileHandle, string message)
            : base(fileHandle, message)
        {
        }
    }
}
