using System;
using Anything.FileSystem;
using Anything.Preview.Icons;
using Anything.Preview.Meta;
using Anything.Preview.Meta.Readers;
using Anything.Preview.Mime;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Server.Abstractions.Graphql;
using Anything.Server.Abstractions.Graphql.Types;
using Anything.Server.Abstractions.Http;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Toolkit.HighPerformance;

namespace Anything.Preview;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddIconsFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IIconsService, IconsService>();

        services.TryAddFileFieldEndpoint<NonNullGraphType<StringGraphType>>(
            "icon",
            "Icon url of the file.",
            resolve: async (FileHandle handle, IIconsService iconsService) =>
                "/api/icons/" + Uri.EscapeDataString(await iconsService.GetIconId(handle)));

        services.TryAddGetEndpoint("/api/icons/{name}", async (string name, int? size, IIconsService icons) =>
        {
            var option = new IconImageOption();
            if (size != null)
            {
                option = option with { Size = size.Value };
            }

            var iconImage = await icons.GetIconImage(name, option);
#pragma warning disable IDISP004
            return Results.Stream(iconImage.Data.AsStream(), iconImage.ImageFormat);
#pragma warning restore IDISP004
        });

        return services;
    }

    public static IServiceCollection TryAddMetadataFeature(this IServiceCollection services, bool defaultReaders = true)
    {
        if (defaultReaders)
        {
            services.TryAddMetadataReader<FileInformationMetadataReader>()
                .TryAddMetadataReader<AudioFileMetadataReader>()
                .TryAddMetadataReader<ImageFileMetadataReader>();
        }

        services.TryAddScoped<IMetadataService, MetadataService>();

        services.TryAddFileFieldEndpoint<JsonGraphType>(
            "metadata",
            "Metadata of the file.",
            resolve: async (FileHandle handle, IMetadataService metadataService) =>
                (await metadataService.ReadMetadata(handle)).ToDictionary());

        return services;
    }

    public static IServiceCollection TryAddMetadataReader<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IMetadataReader
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IMetadataReader, TImplementation>());
        return services;
    }

    public static IServiceCollection TryAddMimeTypeFeature(this IServiceCollection services)
    {
        services.TryAddSingleton(MimeTypeRules.DefaultRules);
        services.TryAddScoped<IMimeTypeService, MimeTypeService>();

        services.TryAddFileFieldEndpoint<StringGraphType>(
            "mime",
            "Media type of the file.",
            resolve: async (FileHandle handle, IMimeTypeService mimeTypeService) => (await mimeTypeService.GetMimeType(handle))?.Mime);

        return services;
    }

    public static IServiceCollection TryAddThumbnailsFeature(this IServiceCollection services, bool defaultRenderers = true)
    {
        if (defaultRenderers)
        {
            services.TryAddThumbnailsRenderer<VideoFileRenderer>()
                .TryAddThumbnailsRenderer<ImageFileRenderer>()
                .TryAddThumbnailsRenderer<TextFileRenderer>()
                .TryAddThumbnailsRenderer<AudioFileRenderer>();
        }

        services.TryAddScoped<IThumbnailsService, ThumbnailsService>();

        services.TryAddFileFieldEndpoint<StringGraphType>(
            "thumbnail",
            "Thumbnail url of the file.",
            resolve: async (FileHandle handle, IThumbnailsService thumbnailsService) =>
                await thumbnailsService.IsSupportThumbnail(handle)
                    ? $"/api/thumbnails?fileHandle={Uri.EscapeDataString(handle.Identifier)}"
                    : null);

        services.TryAddGetEndpoint("/api/thumbnails", async (string? fileHandle, int? size, IThumbnailsService thumbnailsService) =>
        {
            size ??= 256;

            if (fileHandle == null)
            {
                return Results.BadRequest("The \"fileHandle\" argument out of range.");
            }

            var thumbnail = await thumbnailsService.GetThumbnailImage(
                new FileHandle(fileHandle),
                new ThumbnailOption { Size = size.Value });

            if (thumbnail != null)
            {
#pragma warning disable IDISP004
                return Results.Stream(thumbnail.Data.AsStream(), thumbnail.ImageFormat);
#pragma warning restore IDISP004
            }

            return Results.NoContent();
        });

        return services;
    }

    public static IServiceCollection TryAddThumbnailsRenderer<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IThumbnailsRenderer
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IThumbnailsRenderer, TImplementation>());
        return services;
    }
}
