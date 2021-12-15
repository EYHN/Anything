using Anything.FileSystem;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class FileStatsType : ObjectGraphType<FileStats>
{
    public FileStatsType()
    {
        Name = "FileStats";
        Description = "A FileStats object provides information about a file.";

        Field<LongGraphType>(
            "size",
            resolve: d => d.Source!.Size,
            description: "The size of the file in bytes.");
        Field<DateTimeOffsetGraphType>(
            "creationTime",
            resolve: d => d.Source!.CreationTime,
            description: "The creation time of the file.");
        Field<DateTimeOffsetGraphType>(
            "lastWriteTime",
            resolve: d => d.Source!.LastWriteTime,
            description: "The last time this file was modified.");
    }
}
