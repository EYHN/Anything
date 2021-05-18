using System.Linq;
using GraphQL.Types;
using StagingBox.File;
using StagingBox.Preview.Icons;

namespace StagingBox.Server.Api.Graphql.Types
{
    public class DirectoryType : ObjectGraphType<IDirectory>
    {
        public DirectoryType(DynamicIconsService dynamicIconsService)
        {
            Name = "Directory";
            Description = "A directory is a location for storing files.";

            Field<NonNullGraphType<StringGraphType>>("path",
                resolve: d => d.Source.Path,
                description: "Represents the fully qualified path of this directory.");
            Field<NonNullGraphType<StringGraphType>>("name",
                resolve: d => d.Source.Name,
                description: "Name of the directory.");
            FieldAsync<NonNullGraphType<FileStatsType>>("stats",
                resolve: async d => await d.Source.Stats,
                description: "Information about the directory.");
            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<FileInterface>>>>("entries",
                resolve: async d => (await d.Source.Entries).ToList(),
                description: "Entries of the directory.");
            Field<StringGraphType>("mime",
                resolve: d => d.Source.MimeType?.Mime,
                description: "Media type about the directory.");
            Field<NonNullGraphType<StringGraphType>>("icon",
                resolve: d => StaticIconsController.BuildUrl(d.Source.GetIcon()),
                description: "Icon path of the directory.");
            Field<StringGraphType>("dynamicIcon",
                resolve: d =>
                    dynamicIconsService.IsSupported(d.Source) ? DynamicIconsController.BuildUrl(d.Source) : null,
                description: "Dynamic icon path of the directory.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is IDirectory;
        }
    }
}
