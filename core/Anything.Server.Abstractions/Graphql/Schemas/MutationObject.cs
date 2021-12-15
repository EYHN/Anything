using System.Collections.Generic;
using System.Threading.Tasks;
using Anything.Server.Abstractions.Graphql.Endpoint;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Schemas;

internal class MutationObject : ObjectGraphType<object>
{
    public MutationObject(IEnumerable<MutationEndpoint> endpoints)
    {
        Name = "Mutation";
        Description = "The mutation type, represents all updates we can make to our data.";

        foreach (var endpoint in endpoints)
        {
            if (endpoint.IsAsync)
            {
                FieldAsync(
                    endpoint.GraphType,
                    endpoint.Name,
                    endpoint.Description,
                    endpoint.Arguments,
                    context => (Task<object?>)(endpoint.Resolve(null, context, context.RequestServices!)!));
            }
            else
            {
                Field(
                    endpoint.GraphType,
                    endpoint.Name,
                    endpoint.Description,
                    endpoint.Arguments,
                    context => endpoint.Resolve(null, context, context.RequestServices!));
            }
        }
    }
}
