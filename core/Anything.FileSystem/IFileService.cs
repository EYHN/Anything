using Anything.FileSystem.Property;
using Anything.FileSystem.Walker;

namespace Anything.FileSystem;

public interface IFileService : IFileOperations, IPropertyOperations
{
    public IFileSystemWalker CreateWalker(FileHandle rootFileHandle);
}
