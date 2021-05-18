using GraphQL.Types;
using StagingBox.File;

namespace StagingBox.Server.Api.Graphql.Types
{
    public class FileStatsType : ObjectGraphType<IFileStats>
    {
        public FileStatsType()
        {
            Name = "FileStats";
            Description = "A FileStats object provides information about a file.";

            Field<LongGraphType>("size",
                resolve: d => d.Source.Size,
                description: "The size of the file in bytes.");
            Field<DateTimeOffsetGraphType>("modifyTime",
                resolve: d => d.Source.ModifyTime,
                description: "The last time this file was modified.");
            Field<DateTimeOffsetGraphType>("accessTime",
                resolve: d => d.Source.AccessTime,
                description: "The last time this file was accessed.");
            Field<DateTimeOffsetGraphType>("creationTime",
                resolve: d => d.Source.CreationTime,
                description: "The creation time of the file.");
        }
    }
}
