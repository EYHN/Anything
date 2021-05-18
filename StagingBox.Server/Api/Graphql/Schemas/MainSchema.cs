using System;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using StagingBox.Server.Api.Graphql.Types;
using StagingBox.Server.Models;

namespace StagingBox.Server.Api.Graphql.Schemas
{
    public class MainSchema : Schema
    {
        public MainSchema(Application application)
        {
            Query = new QueryObject(application);
            Mutation = new MutationObject();

            RegisterType(new FileInterface());
            RegisterType(new RegularFileType());
            RegisterType(new UnknownFileType());
            RegisterType(new DirectoryType());
            RegisterType(new FileStatsType());

            RegisterType(new JsonGraphType());
        }
    }
}
