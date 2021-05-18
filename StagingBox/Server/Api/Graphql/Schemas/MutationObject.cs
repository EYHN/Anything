using GraphQL;
using GraphQL.Types;

namespace StagingBox.Server.Api.Graphql.Schemas
{
    public class MutationObject : ObjectGraphType<object>
    {
        public MutationObject()
        {
            Name = "Mutation";
            Description = "The mutation type, represents all updates we can make to our data.";

            this.Field<StringGraphType>("Hello",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "World" }
                ),
                resolve: context =>
                {
                    var name = context.GetArgument<string>("name");
                    return name;
                });
        }
    }
}
