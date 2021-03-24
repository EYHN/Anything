using System;
using System.Text.Json;

namespace OwnHub.Sqlite.Triples
{
    [TriplesTypeName("JSON")]
    public class TriplesJsonObject : TriplesObject
    {
        public object? this[string name]
        {
            get => GetProperty(name);
            set
            {
                if (value != null)
                {
                    SetProperty(name, value);
                }
                else
                {
                    DeleteProperty(name);
                }
            }
        }

        public TriplesJsonObject()
        {
        }

        public TriplesJsonObject(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Only accept JSON object.");
            }

            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    this[property.Name] = property.Value.GetString();
                }
            }
        }
    }
}
