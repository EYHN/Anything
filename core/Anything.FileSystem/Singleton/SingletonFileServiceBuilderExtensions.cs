using System;
using Anything.FileSystem.Singleton.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Anything.FileSystem.Singleton;

public static class SingletonFileServiceBuilderExtensions
{
    public static SingletonFileServiceBuilder TryAddFileSystem(
        this SingletonFileServiceBuilder builder,
        FileSystemDescriptor fileSystemDescriptor)
    {
        foreach (var service in builder.ServiceCollection)
        {
            if (service.ServiceType == typeof(FileSystemDescriptor))
            {
                var exists = (FileSystemDescriptor)service.ImplementationInstance!;
                if (exists.NameSpace == fileSystemDescriptor.NameSpace)
                {
                    return builder;
                }
            }
        }

        builder.ServiceCollection.Add(ServiceDescriptor.Singleton(fileSystemDescriptor));
        return builder;
    }

    public static SingletonFileServiceBuilder TryAddFileSystem<TFileSystem>(this SingletonFileServiceBuilder builder, string @namespace)
        where TFileSystem : ISingletonFileSystem
    {
        return builder.TryAddFileSystem(new FileSystemDescriptor(
            @namespace,
            provider => ActivatorUtilities.CreateInstance<TFileSystem>(provider)));
    }

    public static SingletonFileServiceBuilder TryAddFileSystem<TFileSystem>(
        this SingletonFileServiceBuilder builder,
        string @namespace,
        Func<IServiceProvider, TFileSystem> factory)
        where TFileSystem : ISingletonFileSystem
    {
        return builder.TryAddFileSystem(new FileSystemDescriptor(
            @namespace,
            provider => factory(provider)));
    }

    public static SingletonFileServiceBuilder TryAddMemoryFileSystem(this SingletonFileServiceBuilder builder, string @namespace)
    {
        return builder.TryAddFileSystem<MemoryFileSystem>(@namespace);
    }

    public static SingletonFileServiceBuilder TryAddEmbeddedFileSystem(
        this SingletonFileServiceBuilder builder,
        string @namespace,
        EmbeddedFileProvider embeddedFileProvider)
    {
        return builder.TryAddFileSystem(@namespace, _ => new EmbeddedFileSystem(embeddedFileProvider));
    }
}
