using GraphQL.Language.AST;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OwnHub.Server.Api.Graphql.Types
{
    class JsonGraphTypeConverter : IAstFromValueConverter
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

    class JsonGraphValue : ValueNode<JsonElement>
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
        public JsonGraphType() => this.Name = "Json";

        public override object ParseLiteral(IValue value) => value.Value;

        public override object ParseValue(object value)
        {
            var jsonString = JsonSerializer.Serialize(value);
            return JsonDocument.Parse(jsonString).RootElement;
        }

        public override object Serialize(object value) => this.ParseValue(value);
    }
}
