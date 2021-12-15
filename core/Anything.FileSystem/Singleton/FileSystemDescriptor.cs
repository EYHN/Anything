using System;

namespace Anything.FileSystem.Singleton;

public class FileSystemDescriptor
{
    public FileSystemDescriptor(string nameSpace, Func<IServiceProvider, ISingletonFileSystem> implementationFactory)
    {
        NameSpace = nameSpace;
        ImplementationFactory = implementationFactory;
    }

    public string NameSpace { get; }

    public Func<IServiceProvider, ISingletonFileSystem> ImplementationFactory { get; }
}
