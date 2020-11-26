using GraphQL;
using GraphQL.Types;
using OwnHub.File.Local;
using OwnHub.Server.Api.Graphql.Types;

namespace OwnHub.Server.Api.Graphql.Schemas
{
    public class QueryObject : ObjectGraphType
    {
        public QueryObject()
        {
            Name = "Query";
            Description = "The query type, represents all of the entry points into our object graph.";

            Field<NonNullGraphType<DirectoryType>>(
                "openDirectory",
                "Open a directory",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>
                    {
                        Name = "path",
                        Description = "The path of the directory to open"
                    }),
                context => FileSystem.TestFilesystem.OpenDirectory(context.GetArgument("path", "/"))
            );
        }
    }
}