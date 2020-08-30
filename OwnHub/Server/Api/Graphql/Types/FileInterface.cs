using OwnHub.File;
using OwnHub.Preview.Icons;
using GraphQL.Instrumentation;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Server.Api.Graphql.Types
{
    public class FileInterface : InterfaceGraphType<IFile>
    {
        public FileInterface()
        {
            this.Name = "File";
            this.Description = "A File object can represent either a file or a directory.";
            this.Field<NonNullGraphType<StringGraphType>>("path", "Represents the fully qualified path of the directory or file.");
            this.Field<NonNullGraphType<StringGraphType>>("name", "Name of the file.");
            this.Field<NonNullGraphType<FileStatsType>>("stats", "Information about the file.");
            this.Field<StringGraphType>("icon", "Icon name to the media type.");
            this.Field<StringGraphType>("mime", description: "Media type about the file.");
        }
    }
}
