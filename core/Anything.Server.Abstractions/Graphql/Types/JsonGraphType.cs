﻿using System.Text.Json;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class JsonGraphType : ScalarGraphType
{
    public JsonGraphType()
    {
        Name = "Json";
    }

    public override object? ParseLiteral(IValue value)
    {
        return value.Value;
    }

    public override object? ParseValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        var jsonString = JsonSerializer.Serialize(value);
        return JsonDocument.Parse(jsonString).RootElement;
    }

    public override object? Serialize(object? value)
    {
        return ParseValue(value);
    }
}
