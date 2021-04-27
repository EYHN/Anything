using System;
using GraphQL.Types;
using OwnHub.Server.Api.Graphql.Types;

namespace OwnHub.Server.Api.Graphql.Schemas
{
    public class MainSchema : Schema
    {
        public MainSchema(IServiceProvider provider)
            : base(provider)
        {
            Query = new QueryObject();
            Mutation = new MutationObject();

            RegisterType(typeof(FileInterface));
            RegisterType(typeof(RegularFileType));
            RegisterType(typeof(DirectoryType));
            RegisterType(typeof(FileStatsType));

            RegisterType(typeof(JsonGraphType));
        }
    }
}
