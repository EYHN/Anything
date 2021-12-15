using System;
using Anything.Utils;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class UrlGraphType : ScalarGraphType
{
    public UrlGraphType()
    {
        Name = "Url";
    }

    public override object? ParseLiteral(IValue value)
    {
        if (value is NullValue)
        {
            return null;
        }

        if (value is StringValue stringValue)
        {
            return ParseValue(stringValue.Value);
        }

        return ThrowLiteralConversionError(value);
    }

    public override object? ParseValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string urlString)
        {
            try
            {
                return Url.Parse(urlString);
            }
            catch
            {
                throw new FormatException($"Failed to parse {nameof(Url)} from input '{urlString}'.");
            }
        }

        return ThrowValueConversionError(value);
    }

    public override object? Serialize(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is Url url)
        {
            return url.ToString();
        }

        return ThrowSerializationError(value);
    }
}
