using GraphQL.Types;
using OwnHub.File;
using OwnHub.Preview.Icons;
using OwnHub.Preview.Metadata;

namespace OwnHub.Server.Api.Graphql.Types
{
    public class RegularFileType : ObjectGraphType<IRegularFile>
    {
        public RegularFileType(DynamicIconsService dynamicIconsService, MetadataService metadataService)
        {
            Name = "RegularFile";
            Description =
                "A regular file is a file that is not a directory and is not some special kind of file such as a device.";

            Field<NonNullGraphType<StringGraphType>>("path",
                resolve: d => d.Source.Path,
                description: "Represents the fully qualified path of this file.");
            Field<NonNullGraphType<StringGraphType>>("name",
                resolve: d => d.Source.Name,
                description: "The name of the file.");
            FieldAsync<NonNullGraphType<FileStatsType>>("stats",
                resolve: async d => await d.Source.Stats,
                description: "Information about the file.");
            Field<StringGraphType>("mime",
                resolve: d => d.Source.MimeType?.Mime,
                description: "Media type about the file.");
            Field<NonNullGraphType<StringGraphType>>("icon",
                resolve: d => StaticIconsController.BuildUrl(d.Source.GetIcon()),
                description: "Icon path of the file.");
            Field<StringGraphType>("dynamicIcon",
                resolve: d =>
                    dynamicIconsService.IsSupported(d.Source) ? DynamicIconsController.BuildUrl(d.Source) : null,
                description: "Dynamic icon path of the file.");
            FieldAsync<JsonGraphType>("metadata",
                resolve: async d => (await metadataService.ReadMetadata(d.Source)).ToDictionary(),
                description: "Metadata of the file.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is IRegularFile;
        }
    }
}