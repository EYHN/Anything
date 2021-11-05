using Anything.FileSystem;
using Anything.Server.Api.Graphql.Types;
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

            FieldAsync<NonNullGraphType<FileHandleRefType>>(
                "createFileHandle",
                "Create a file handle by url.",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<UrlGraphType>> { Name = "url", Description = "The url to create file handle." }),
                async context => await context.GetApplication().CreateFileHandle(context.GetArgument<Url>("url")!));

            FieldAsync<NonNullGraphType<FileHandleRefType>>(
                "openFileHandle",
                "Open a file handle",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<FileHandleGraphType>>
                    {
                        Name = "fileHandle", Description = "The file handle to open."
                    }),
                async context => await context.GetApplication().OpenFileHandle(context.GetArgument<FileHandle>("fileHandle")!));

            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<FileInterface>>>>(
                "search",
                "Search",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "q", Description = "The search string to search." },
                    new QueryArgument<UrlGraphType> { Name = "baseUrl", Description = "The path on which the search is based." }),
                async context =>
                    await context.GetApplication().Search(context.GetArgument<string>("q")!, context.GetArgument<Url>("baseUrl")));
        }
    }
}
