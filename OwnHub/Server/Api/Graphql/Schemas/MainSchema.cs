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

            RegisterType<FileInterface>();
            RegisterType<RegularFileType>();
            RegisterType<DirectoryType>();
            RegisterType<FileStatsType>();

            RegisterValueConverter(new JsonGraphTypeConverter());
        }
    }
}