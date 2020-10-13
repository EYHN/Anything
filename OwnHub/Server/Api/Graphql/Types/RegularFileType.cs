using OwnHub.File;
using OwnHub.Preview.Icons;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwnHub.Preview.Metadata;

namespace OwnHub.Server.Api.Graphql.Types
{
    public class RegularFileType : ObjectGraphType<IRegularFile>
    {
        public RegularFileType(DynamicIconsService DynamicIconsService, MetadataService metadataService)
        {
            this.Name = "RegularFile";
            this.Description = "A regular file is a file that is not a directory and is not some special kind of file such as a device.";

            this.Field<NonNullGraphType<StringGraphType>>("path",
                resolve: d => d.Source.Path,
                description: "Represents the fully qualified path of this file.");
            this.Field<NonNullGraphType<StringGraphType>>("name",
                resolve: d => d.Source.Name,
                description: "The name of the file.");
            this.FieldAsync<NonNullGraphType<FileStatsType>>("stats",
                resolve: async d => await d.Source.Stats,
                description: "Information about the file.");
            this.Field<StringGraphType>("mime",
                resolve: d => d.Source.MimeType?.Mime,
                description: "Media type about the file.");
            this.Field<NonNullGraphType<StringGraphType>>("icon",
                resolve: d => StaticIconsController.BuildUrl(d.Source.GetIcon()),
                description: "Icon path of the file.");
            this.Field<StringGraphType>("dynamicIcon",
                resolve: d => DynamicIconsService.IsSupported(d.Source) ? DynamicIconsController.BuildUrl(d.Source) : null,
                description: "Dynamic icon path of the file.");
            this.Field<JsonGraphType>("metadata",
                resolve: d => metadataService.ReadImageMetadata(d.Source),
                description: "Metadata of the file.");

            this.Interface<FileInterface>();

            this.IsTypeOf = obj => obj is IRegularFile;
        }
    }
}
