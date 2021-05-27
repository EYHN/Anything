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

            Field<NonNullGraphType<UrlGraphType>>(
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
            FieldAsync<NonNullGraphType<StringGraphType>>(
                "icon",
                resolve: async d => IconsController.BuildUrl(await d.Source.IconId),
                description: "Icon path of the file.");
            FieldAsync<StringGraphType>(
                "thumbnail",
                resolve: async d =>
                    await d.Source.IsSupportThumbnails ? ThumbnailsController.BuildUrl(d.Source.Url) : null,
                description: "Thumbnail path of the file.");

            Interface<FileInterface>();

            IsTypeOf = obj => obj is RegularFile;
        }
    }
}
