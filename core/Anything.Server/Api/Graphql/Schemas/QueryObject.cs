using Anything.Server.Api.Graphql.Types;
using Anything.Server.Models;
using Anything.Utils;
using GraphQL;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Schemas
{
    public class QueryObject : ObjectGraphType
    {
        public QueryObject()
        {
            Name = "Query";
            Description = "The query type, represents all of the entry points into our object graph.";

            FieldAsync<NonNullGraphType<DirectoryType>>(
                "directory",
                "Query a directory",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<UrlGraphType>> { Name = "url", Description = "The url of the directory." }),
                async context => await context.GetApplication().OpenDirectory(context.GetArgument<Url>("url")));

            FieldAsync<NonNullGraphType<FileInterface>>(
                "file",
                "Query a file",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<UrlGraphType>> { Name = "url", Description = "The url of the file." }),
                async context => await context.GetApplication().Open(context.GetArgument<Url>("url")));

            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<FileInterface>>>>(
                "search",
                "Search",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "q", Description = "The search string to search." },
                    new QueryArgument<UrlGraphType> { Name = "baseUrl", Description = "The path on which the search is based." }),
                async context =>
                    await context.GetApplication().Search(context.GetArgument<string>("q"), context.GetArgument<Url>("baseUrl")));
        }
    }
}
