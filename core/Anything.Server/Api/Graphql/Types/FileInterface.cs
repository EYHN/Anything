using Anything.Server.Models;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Types
{
    public class FileInterface : InterfaceGraphType<File>
    {
        public FileInterface()
        {
            Name = "File";
            Description = "A File object can represent either a file or a directory.";
            Field<NonNullGraphType<FileHandleRefType>>(
                "fileHandle",
                "File handle of the file.");
            Field<NonNullGraphType<StringGraphType>>(
                "name",
                "Name of the file.");
            Field<NonNullGraphType<UrlGraphType>>(
                "url",
                "Url of the file.");
            Field<NonNullGraphType<FileStatsType>>("stats", "Information about the file.");
            Field<NonNullGraphType<StringGraphType>>("icon", "Icon path of the file.");
            Field<StringGraphType>("mime", "Media type about the file.");
            Field<StringGraphType>("thumbnail", "Thumbnail path of the file.");
            Field<JsonGraphType>("metadata", "Metadata of the directory.");
        }
    }
}
