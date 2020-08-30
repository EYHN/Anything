using OwnHub.Server.Api.Graphql.Types;
using GraphQL;
using GraphQL.Types;
using System;

namespace OwnHub.Server.Api.Graphql.Schemas
{
    public class MutationObject : ObjectGraphType<object>
    {
        public MutationObject()
        {
            this.Name = "Mutation";
            this.Description = "The mutation type, represents all updates we can make to our data.";

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
