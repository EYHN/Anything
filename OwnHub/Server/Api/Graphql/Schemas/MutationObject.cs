using GraphQL.Types;

namespace OwnHub.Server.Api.Graphql.Schemas
{
    public class MutationObject : ObjectGraphType<object>
    {
        public MutationObject()
        {
            Name = "Mutation";
            Description = "The mutation type, represents all updates we can make to our data.";

            //this.Field<HumanObject>("addHuman",
            //    arguments: new QueryArguments(
            //        new QueryArgument<StringGraphType> { Name = "name" }
            //    ),
            //    resolve: context =>
            //    {
            //        var name = context.GetArgument<string>("name");
            //        return new Human(name);
            //    });
        }
    }
}