using GraphQL;
using GraphQL.Types;
using StagingBox.Server.Api.Graphql.Types;
using StagingBox.Server.Models;
using StagingBox.Utils;

namespace StagingBox.Server.Api.Graphql.Schemas
{
    public class QueryObject : ObjectGraphType
    {
        public QueryObject(Application application)
        {
            Name = "Query";
            Description = "The query type, represents all of the entry points into our object graph.";

            FieldAsync<NonNullGraphType<DirectoryType>>(
                "directory",
                "Query a directory",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "url", Description = "The url of the directory." }),
                async context => await application.OpenDirectory(Url.Parse(context.GetArgument("url", "/"))));

            FieldAsync<NonNullGraphType<FileInterface>>(
                "file",
                "Query a file",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "url", Description = "The url of the file." }),
                async context => await application.Open(Url.Parse(context.GetArgument("url", "/"))));
        }
    }
}
