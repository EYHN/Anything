using System.IO;

namespace OwnHub.File
{
    public interface IRegularFile : IFile
    {
        public Stream Open();
    }
}