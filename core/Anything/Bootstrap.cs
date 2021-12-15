using System.IO;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.FileSystem.Singleton.Impl;
using Anything.FileSystem.Singleton.Tracker;
using Anything.Notes;
using Anything.Preview;
using Anything.Tags;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Anything;

public static class Bootstrap
{
    public static void ConfigureLogging(ILoggingBuilder builder)
    {
        builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
    }

    public static void ConfigureFeature(IServiceCollection services)
    {
        // preview features
        services
            .TryAddIconsFeature()
            .TryAddMetadataFeature()
            .TryAddThumbnailsFeature()
            .TryAddMimeTypeFeature();

        // other features
        services
            .TryAddNoteFeature()
            .TryAddTagFeature();
    }

    public static void ConfigureFileService(IServiceCollection services)
    {
        services
            .TryAddSingletonFileService(builder =>
                builder.TryAddMemoryFileSystem("memory")
                    .TryAddFileSystem(
                        "local",
                        provider => new LocalFileSystem(
                            Path.GetFullPath("./Test"),
                            provider.GetRequiredService<IFileEventService>(),
                            provider.GetRequiredService<ILogger<LocalFileSystem>>())));
    }
}
