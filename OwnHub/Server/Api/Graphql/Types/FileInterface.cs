using GraphQL.Types;
using OwnHub.File;

namespace OwnHub.Server.Api.Graphql.Types
{
    public class FileInterface : InterfaceGraphType<IFile>
    {
        public FileInterface()
        {
            Name = "File";
            Description = "A File object can represent either a file or a directory.";
            Field<NonNullGraphType<StringGraphType>>("path",
                "Represents the fully qualified path of the directory or file.");
            Field<NonNullGraphType<StringGraphType>>("name", "Name of the file.");
            Field<NonNullGraphType<FileStatsType>>("stats", "Information about the file.");
            Field<NonNullGraphType<StringGraphType>>("icon", "Icon path of the file.");
            Field<StringGraphType>("mime", "Media type about the file.");
            Field<StringGraphType>("dynamicIcon", "Dynamic icon path of the file.");
        }
    }
}