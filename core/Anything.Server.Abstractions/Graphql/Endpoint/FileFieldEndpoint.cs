using System;
using Anything.FileSystem;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Endpoint;

public class FileFieldEndpoint : GraphQlEndpoint
{
    internal FileFieldEndpoint(string name, string? description, Type graphType, Delegate resolve, QueryArguments? arguments = null)
        : base(typeof(FileHandle), name, description, graphType, resolve, arguments)
    {
    }
}
