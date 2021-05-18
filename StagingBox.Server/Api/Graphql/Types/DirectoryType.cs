using System.Linq;
using GraphQL.Types;
using StagingBox.Server.Models;

namespace StagingBox.Server.Api.Graphql.Types
{
    public class DirectoryType : ObjectGraphType<Directory>
    {
        public DirectoryType()
        {
            Name = "Directory";
            Description = "A directory is a location for storing files.";

            Field<NonNullGraphType<StringGraphType>>(
                "url",
                resolve: d => d.Source.Url.ToString(),
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
            Field<NonNullGraphType<StringGraphType>>(
                "icon",
                resolve: d => IconsController.BuildUrl(d.Source.Url),
                description: "Icon path of the directory.");
            Field<StringGraphType>(
                "dynamicIcon",
                resolve: d =>
                    ThumbnailsController.BuildUrl(d.Source.Url),
                description: "Dynamic icon path of the directory.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is Directory;
        }
    }
}
