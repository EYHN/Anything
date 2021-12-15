using System.Collections.Generic;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Server.Abstractions.Graphql.Endpoint;
using Anything.Server.Abstractions.Graphql.Types;
using Anything.Utils;
using GraphQL;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Schemas;

internal class QueryObject : ObjectGraphType
{
    public QueryObject(IEnumerable<QueryEndpoint> endpoints)
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
                new QueryArgument<NonNullGraphType<FileHandleGraphType>> { Name = "fileHandle", Description = "The file handle to open." }),
            async context => await context.GetApplication().OpenFileHandle(context.GetArgument<FileHandle>("fileHandle")!));

        foreach (var endpoint in endpoints)
        {
            if (endpoint.IsAsync)
            {
                FieldAsync(
                    endpoint.GraphType,
                    endpoint.Name,
                    endpoint.Description,
                    endpoint.Arguments,
                    context => (Task<object?>)(endpoint.Resolve(null, context, context.RequestServices!)!));
            }
            else
            {
                Field(
                    endpoint.GraphType,
                    endpoint.Name,
                    endpoint.Description,
                    endpoint.Arguments,
                    context => endpoint.Resolve(null, context, context.RequestServices!));
            }
        }

        // FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<FileInterface>>>>(
        //     "search",
        //     "Search",
        //     new QueryArguments(
        //         new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "q", Description = "The search string to search." },
        //         new QueryArgument<UrlGraphType> { Name = "baseUrl", Description = "The path on which the search is based." }),
        //     async context =>
        //         await context.GetApplication().Search(context.GetArgument<string>("q")!, context.GetArgument<Url>("baseUrl")));
    }
}
