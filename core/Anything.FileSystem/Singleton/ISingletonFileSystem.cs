using Anything.FileSystem.Property;
using Anything.FileSystem.Walker;

namespace Anything.FileSystem.Singleton;

public interface ISingletonFileSystem : IFileOperations, IPropertyOperations
{
    public IFileSystemWalker CreateWalker(FileHandle rootFileHandle);
}
