using OwnHub.File;
using OwnHub.Preview.Icons;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Server.Api.Graphql.Types
{
    public class DirectoryType : ObjectGraphType<IDirectory>
    {
        public DirectoryType(DynamicIconsService DynamicIconsService)
        {
            this.Name = "Directory";
            this.Description = "A directory is a location for storing files.";

            this.Field<NonNullGraphType<StringGraphType>>("path",
                resolve: d => d.Source.Path,
                description: "Represents the fully qualified path of this directory.");
            this.Field<NonNullGraphType<StringGraphType>>("name",
                resolve: d => d.Source.Name,
                description: "Name of the directory.");
            this.FieldAsync<NonNullGraphType<FileStatsType>>("stats",
                resolve: async d => await d.Source.Stats,
                description: "Information about the directory.");
            this.FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<FileInterface>>>>("entries",
                resolve: async d => (await d.Source.Entries).ToList(),
                description: "Entries of the directory.");
            this.Field<StringGraphType>("mime",
                resolve: d => d.Source.MimeType?.Mime,
                description: "Media type about the directory.");
            this.Field<NonNullGraphType<StringGraphType>>("icon",
                resolve: d => StaticIconsController.BuildUrl(d.Source.GetIcon()),
                description: "Icon path of the directory.");
            this.Field<StringGraphType>("dynamicIcon",
                resolve: d => DynamicIconsService.IsSupported(d.Source) ? DynamicIconsController.BuildUrl(d.Source) : null,
                description: "Dynamic icon path of the directory.");

            this.Interface<FileInterface>();

            this.IsTypeOf = obj => obj is IDirectory;
        }
    }
}
