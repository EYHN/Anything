using System.Text.Json;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace OwnHub.Server.Api.Graphql.Types
{
    internal class JsonGraphTypeConverter : IAstFromValueConverter
    {
        public bool Matches(object value, IGraphType type)
        {
            return type.Name == "Json";
        }

        public IValue Convert(object value, IGraphType type)
        {
            return new JsonGraphValue((JsonElement) value);
        }
    }

    internal class JsonGraphValue : ValueNode<JsonElement>
    {
        public JsonGraphValue(JsonElement value)
        {
            Value = value;
        }

        protected override bool Equals(ValueNode<JsonElement> node)
        {
            return false;
        }
    }
 
    internal class JsonGraphType : ScalarGraphType
    {
        public JsonGraphType()
        {
            Name = "Json";
        }

        public override object ParseLiteral(IValue value)
        {
            return value.Value;
        }

        public override object ParseValue(object value)
        {
            string? jsonString = JsonSerializer.Serialize(value);
            return JsonDocument.Parse(jsonString).RootElement;
        }

        public override object Serialize(object value)
        {
            return ParseValue(value);
        }
    }
}