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

            Field<NonNullGraphType<UrlGraphType>>(
                "url",
                resolve: d => d.Source.Url,
                description: "Represents the fully qualified url of this directory.");
            Field<NonNullGraphType<StringGraphType>>(
                "name",
                resolve: d => d.Source.Name,
                description: "Name of the directory.");
            Field<NonNullGraphType<FileStatsType>>(
                "stats",
                resolve: d => d.Source.Stats,
                description: "Information about the directory.");
            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<FileInterface>>>>(
                "entries",
                resolve: async d => (await d.Source.Entries).ToArray(),
                description: "Entries of the directory.");
            FieldAsync<StringGraphType>(
                "mime",
                resolve: async d => await d.Source.MimeType,
                description: "Media type about the directory.");
            FieldAsync<NonNullGraphType<StringGraphType>>(
                "icon",
                resolve: async d => IconsController.BuildUrl(await d.Source.IconId),
                description: "Icon path of the directory.");
            FieldAsync<StringGraphType>(
                "thumbnail",
                resolve: async d =>
                    await d.Source.IsSupportThumbnails ? ThumbnailsController.BuildUrl(d.Source.Url) : null,
                description: "Thumbnail path of the directory.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is Directory;
        }
    }
}
