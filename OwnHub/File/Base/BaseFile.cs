using System.Threading.Tasks;

namespace OwnHub.File.Base
{
    public abstract class BaseFile : IFile
    {
        public abstract string Path { get; }

        public abstract string Name { get; }

        public abstract MimeType? MimeType { get; }

        public abstract Task<IFileStats?> Stats { get; }
    }
}