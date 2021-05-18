using GraphQL;
using GraphQL.Types;
using StagingBox.File.Local;
using StagingBox.Preview.Metadata;
using StagingBox.Server.Api.Graphql.Types;

namespace StagingBox.Server.Api.Graphql.Schemas
{
    public class QueryObject : ObjectGraphType
    {
        public QueryObject()
        {
            Name = "Query";
            Description = "The query type, represents all of the entry points into our object graph.";

            Field<NonNullGraphType<DirectoryType>>(
                "directory",
                "Query a directory",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>
                    {
                        Name = "path",
                        Description = "The path of the directory."
                    }),
                context => FileSystem.TestFilesystem.OpenDirectory(context.GetArgument("path", "/"))
            );

            Field<NonNullGraphType<FileInterface>>(
                "file",
                "Query a file",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>
                    {
                        Name = "path",
                        Description = "The path of the file."
                    }),
                context => FileSystem.TestFilesystem.Open(context.GetArgument("path", "/"))
            );

            Field<NonNullGraphType<ListGraphType<StringGraphType>>>(
                "metadataList",
                "List all supported metadata.",
                new QueryArguments(),
                context => MetadataEntry.ToMetadataNamesList()
            );
        }
    }
}
