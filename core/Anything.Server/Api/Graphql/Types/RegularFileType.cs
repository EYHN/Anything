using System.Linq;
using Anything.Server.Models;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Types
{
    public class RegularFileType : ObjectGraphType<RegularFile>
    {
        public RegularFileType()
        {
            Name = "RegularFile";
            Description =
                "A regular file is a file that is not a directory and is not some special kind of file such as a device.";

            Field<NonNullGraphType<StringGraphType>>(
                "_id",
                resolve: d => d.Source.FileHandle.Value.Identifier,
                description: "Identifier for this file. Same as fileHandle.value.identifier");
            Field<NonNullGraphType<FileHandleRefType>>(
                "fileHandle",
                resolve: d => d.Source.FileHandle,
                description: "File handle of the file.");
            FieldAsync<NonNullGraphType<StringGraphType>>(
                "name",
                resolve: async d => await d.Source.GetFileName(),
                description: "Name of the file.");
            FieldAsync<NonNullGraphType<UrlGraphType>>(
                "url",
                resolve: async d => await d.Source.GetUrl(),
                description: "Url of the file.");
            Field<NonNullGraphType<FileStatsType>>(
                "stats",
                resolve: d => d.Source.Stats,
                description: "Information about the file.");
            FieldAsync<StringGraphType>(
                "mime",
                resolve: async d => (await d.Source.GetMimeType())?.Mime,
                description: "Media type about the file.");
            FieldAsync<NonNullGraphType<StringGraphType>>(
                "icon",
                resolve: async d => IconsController.BuildUrl(await d.Source.GetIconId()),
                description: "Icon path of the file.");
            FieldAsync<StringGraphType>(
                "thumbnail",
                resolve: async d =>
                    await d.Source.IsSupportThumbnails() ? ThumbnailsController.BuildUrl(d.Source.FileHandle.Value) : null,
                description: "Thumbnail path of the file.");
            FieldAsync<JsonGraphType>(
                "metadata",
                resolve: async d => (await d.Source.GetMetadata()).ToDictionary(),
                description: "Metadata of the file.");
            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<StringGraphType>>>>(
                "tags",
                resolve: async d => (await d.Source.GetTags()).Select(t => t.Name),
                description: "Tags of the file.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is RegularFile;
        }
    }
}
