using Anything.Server.Api.Graphql.Types;
using Anything.Server.Models;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Schemas
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
            RegisterType(new UrlGraphType());
        }
    }
}
