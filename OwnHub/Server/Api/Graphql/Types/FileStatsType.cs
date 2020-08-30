using OwnHub.File;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Server.Api.Graphql.Types
{
    public class FileStatsType : ObjectGraphType<IFileStats>
    {
        public FileStatsType()
        {
            this.Name = "FileStats";
            this.Description = "A FileStats object provides information about a file.";

            this.Field<LongGraphType>("size",
                resolve: d => d.Source.Size,
                description: "The size of the file in bytes.");
            this.Field<DateTimeOffsetGraphType>("modifyTime",
                resolve: d => d.Source.ModifyTime,
                description: "The last time this file was modified.");
            this.Field<DateTimeOffsetGraphType>("accessTime",
                resolve: d => d.Source.AccessTime,
                description: "The last time this file was accessed.");
            this.Field<DateTimeOffsetGraphType>("creationTime",
                resolve: d => d.Source.CreationTime,
                description: "The creation time of the file.");
        }
    }
}
