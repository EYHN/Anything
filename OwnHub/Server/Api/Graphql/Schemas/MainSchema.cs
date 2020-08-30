using OwnHub.Server.Api.Graphql.Types;
using GraphQL;
using GraphQL.Types;
using System;

namespace OwnHub.Server.Api.Graphql.Schemas
{
    public class MainSchema: Schema
    {
        public MainSchema(IServiceProvider provider)
            : base(provider)
        {
            this.Query = new QueryObject();
            this.Mutation = new MutationObject();

            this.RegisterType<FileInterface>();
            this.RegisterType<RegularFileType>();
            this.RegisterType<DirectoryType>();
            this.RegisterType<FileStatsType>();
        }
    }
}
