using OwnHub.File;
using OwnHub.Preview.Icons;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Server.Api.Graphql.Types
{
    public class RegularFileType : ObjectGraphType<IRegularFile>
    {
        public RegularFileType(MimeTypeRules mimeTypeRules)
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
                resolve: d => d.Source.MimeType.Mime,
                description: "Media type about the file.");
            this.Field<StringGraphType>("icon",
                resolve: d => StaticIconsController.BuildUrl(d.Source.GetIcon()),
                description: "Icon name to the media type.");

            this.Interface<FileInterface>();

            this.IsTypeOf = obj => obj is IRegularFile;
        }
    }
}
