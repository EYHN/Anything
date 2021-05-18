using System.Threading.Tasks;

namespace StagingBox.File
{
    public interface IFile
    {
        public string Path { get; }

        public string Name { get; }

        public MimeType? MimeType { get; }

        public Task<IFileStats?> Stats { get; }
    }
}
