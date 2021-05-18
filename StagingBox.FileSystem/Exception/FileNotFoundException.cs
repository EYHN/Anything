using StagingBox.Utils;

namespace StagingBox.FileSystem.Exception
{
    public class FileNotFoundException : FileSystemException
    {
        public FileNotFoundException()
        {
        }

        public FileNotFoundException(Url? uri)
            : base(uri)
        {
        }

        public FileNotFoundException(string? message)
            : base(message)
        {
        }

        public FileNotFoundException(Url? uri, string? message)
            : base(uri, message)
        {
        }
    }
}
