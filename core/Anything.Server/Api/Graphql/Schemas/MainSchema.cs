using System;
using Anything.Server.Api.Graphql.Types;
using Anything.Server.Models;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Schemas
{
    public class MainSchema : Schema
    {
        public MainSchema()
        {
            Build();
        }

        public MainSchema(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Build();
        }

        private void Build()
        {
            Query = new QueryObject();
            Mutation = new MutationObject();

            RegisterType(new FileInterface());
            RegisterType(new RegularFileType());
            RegisterType(new UnknownFileType());
            RegisterType(new DirectoryType());
            RegisterType(new FileStatsType());
            RegisterType(new DirentType());

            RegisterType(new JsonGraphType());
            RegisterType(new UrlGraphType());
            RegisterType(new FileHandleGraphType());
        }
    }
}
