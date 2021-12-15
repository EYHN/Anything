using System;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Endpoint;

public class MutationEndpoint : GraphQlEndpoint
{
    internal MutationEndpoint(string name, string? description, Type graphType, Delegate resolve, QueryArguments? arguments = null)
        : base(null, name, description, graphType, resolve, arguments)
    {
    }
}
