using OwnHub.File.Local;
using OwnHub.Server.Api.Graphql.Types;
using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Server.Api.Graphql.Schemas
{
    public class QueryObject : ObjectGraphType
    {
        public QueryObject()
        {
            this.Name = "Query";
            this.Description = "The query type, represents all of the entry points into our object graph.";

            this.Field<NonNullGraphType<DirectoryType>>(
                "openDirectory",
                "Open a directory",
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>()
                    {
                        Name = "path",
                        Description = "The path of the directory to open",
                    }),
                resolve: context => FileSystem._test_filesystem.OpenDirectory(context.GetArgument("path", "/")));
        }
    }
}
