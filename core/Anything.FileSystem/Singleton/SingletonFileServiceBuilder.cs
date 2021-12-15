using Microsoft.Extensions.DependencyInjection;

namespace Anything.FileSystem.Singleton;

public class SingletonFileServiceBuilder
{
    public SingletonFileServiceBuilder(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }
}
