using System.IO;

namespace StagingBox.File
{
    public interface IRegularFile : IFile
    {
        public Stream Open();
    }
}
