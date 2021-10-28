using System.Linq;
using Anything.Server.Models;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Types
{
    public class DirectoryType : ObjectGraphType<Directory>
    {
        public DirectoryType()
        {
            Name = "Directory";
            Description = "A directory is a location for storing files.";

            Field<NonNullGraphType<StringGraphType>>(
                "_id",
                resolve: d => d.Source.FileHandle.Value.Identifier,
                description: "Identifier for this directory. Same as fileHandle.value.identifier");
            Field<NonNullGraphType<FileHandleRefType>>(
                "fileHandle",
                resolve: d => d.Source.FileHandle,
                description: "File handle of the directory.");
            FieldAsync<NonNullGraphType<StringGraphType>>(
                "name",
                resolve: async d => await d.Source.GetFileName(),
                description: "Name of the directory.");
            FieldAsync<NonNullGraphType<UrlGraphType>>(
                "url",
                resolve: async d => await d.Source.GetUrl(),
                description: "Url of the directory.");
            Field<NonNullGraphType<FileStatsType>>(
                "stats",
                resolve: d => d.Source.Stats,
                description: "Information about the directory.");
            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<DirentType>>>>(
                "entries",
                resolve: async d => (await d.Source.ReadEntries()).ToArray(),
                description: "Entries of the directory.");
            FieldAsync<StringGraphType>(
                "mime",
                resolve: async d => (await d.Source.GetMimeType())?.Mime,
                description: "Media type about the directory.");
            FieldAsync<NonNullGraphType<StringGraphType>>(
                "icon",
                resolve: async d => IconsController.BuildUrl(await d.Source.GetIconId()),
                description: "Icon path of the directory.");
            FieldAsync<StringGraphType>(
                "thumbnail",
                resolve: async d =>
                    await d.Source.IsSupportThumbnails() ? ThumbnailsController.BuildUrl(d.Source.FileHandle.Value) : null,
                description: "Thumbnail path of the directory.");
            FieldAsync<JsonGraphType>(
                "metadata",
                resolve: async d => (await d.Source.GetMetadata()).ToDictionary(),
                description: "Metadata of the directory.");
            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<StringGraphType>>>>(
                "tags",
                resolve: async d => (await d.Source.GetTags()).Select(t => t.Name),
                description: "Tags of the directory.");
            FieldAsync<NonNullGraphType<StringGraphType>>(
                "note",
                resolve: async d => await d.Source.GetNote(),
                description: "Note of the directory.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is Directory;
        }
    }
}
