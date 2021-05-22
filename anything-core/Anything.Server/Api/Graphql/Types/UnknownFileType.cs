using Anything.Server.Models;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Types
{
    public class UnknownFileType : ObjectGraphType<UnknownFile>
    {
        public UnknownFileType()
        {
            Name = "UnknownFile";
            Description =
                "A file whose type is not supported.";

            Field<NonNullGraphType<StringGraphType>>(
                "url",
                resolve: d => d.Source.Url,
                description: "Represents the fully qualified url of this file.");
            Field<NonNullGraphType<StringGraphType>>(
                "name",
                resolve: d => d.Source.Name,
                description: "The name of the file.");
            Field<NonNullGraphType<FileStatsType>>(
                "stats",
                resolve: d => d.Source.Stats,
                description: "Information about the file.");
            FieldAsync<StringGraphType>(
                "mime",
                resolve: async d => await d.Source.MimeType,
                description: "Media type about the file.");
            Field<NonNullGraphType<StringGraphType>>(
                "icon",
                resolve: d => IconsController.BuildUrl(d.Source.Url),
                description: "Icon path of the file.");
            Field<StringGraphType>(
                "dynamicIcon",
                resolve: d =>
                    ThumbnailsController.BuildUrl(d.Source.Url),
                description: "Dynamic icon path of the file.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is UnknownFile;
        }
    }
}
