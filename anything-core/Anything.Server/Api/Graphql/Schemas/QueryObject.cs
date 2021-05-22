using Anything.Server.Api.Graphql.Types;
using Anything.Server.Models;
using Anything.Utils;
using GraphQL;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Schemas
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
