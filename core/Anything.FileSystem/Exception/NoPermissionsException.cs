using Anything.Utils;

namespace Anything.FileSystem.Exception
{
    public class NoPermissionsException : FileSystemException
    {
        public NoPermissionsException(Url uri)
            : base(uri)
        {
        }

        public NoPermissionsException(FileHandle fileHandle)
            : base(fileHandle)
        {
        }

        public NoPermissionsException(string message)
            : base(message)
        {
        }

        public NoPermissionsException(Url uri, string message)
            : base(uri, message)
        {
        }

        public NoPermissionsException(FileHandle fileHandle, string message)
            : base(fileHandle, message)
        {
        }
    }
}
