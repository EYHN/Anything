using System.Collections.Generic;
using System.Linq;
using Anything.FileSystem;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Types
{
    public class FileHandleGraphType : ScalarGraphType
    {
        public FileHandleGraphType()
        {
            Name = "FileHandle";
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

            if (value is ObjectValue objectValue)
            {
                var entries = objectValue.ObjectFields.ToDictionary(x => x.Name, x =>
                {
                    if (x.Value is StringValue s)
                    {
                        return s.Value;
                    }

                    return null;
                });

                if (!entries.TryGetValue("identifier", out var identifier) || identifier == null)
                {
                    return ThrowLiteralConversionError(value);
                }

                return new FileHandle(identifier);
            }

            return ThrowLiteralConversionError(value);
        }

        public override object? ParseValue(object? value)
        {
            if (value == null)
            {
                return null;
            }

            {
                if (value is string identifier)
                {
                    return new FileHandle(identifier);
                }
            }

            {
                if (value is Dictionary<string, object> dictionary && dictionary.TryGetValue("identifier", out var identifierVar) &&
                    identifierVar is string identifier)
                {
                    return new FileHandle(identifier);
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

            if (value is FileHandle fileHandle)
            {
                return new { identifier = fileHandle.Identifier };
            }

            return ThrowSerializationError(value);
        }
    }
}
