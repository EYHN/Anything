using System;
using Anything.FileSystem.Singleton;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Anything.FileSystem;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddSingletonFileService(
        this IServiceCollection services,
        Action<SingletonFileServiceBuilder> builder)
    {
        services.TryAddSingleton<IFileService, SingletonFileService>();
        services.TryAddSingleton<IFileEventService, SingletonFileEventService>();
        builder(new SingletonFileServiceBuilder(services));
        return services;
    }

    public static IServiceCollection TryAddFileEventHandler<TFileEventHandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TFileEventHandler : IFileEventHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IFileEventHandler), typeof(TFileEventHandler), lifetime));
        return services;
    }
}
