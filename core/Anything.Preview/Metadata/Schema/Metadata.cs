using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Anything.Preview.Metadata.Schema
{
    public partial class Metadata
    {
        private static List<string> ToMetadataNamesList(
            string? parent,
            bool parentAdvanced,
            List<string> outList,
            Type metadataType)
        {
            var type = metadataType;
            foreach (var property in type.GetProperties())
            {
                var name = property.Name;
                var advanced = parentAdvanced || (property.GetCustomAttribute<MetadataAdvancedAttribute>()?.Advanced ?? false);

                if (typeof(IMetadata).IsAssignableFrom(property.PropertyType))
                {
                    ToMetadataNamesList(parent != null ? parent + "." + name : name, advanced, outList, property.PropertyType);
                }
                else
                {
                    outList.Add((advanced ? "[Advanced] " : "") + (parent != null ? parent + "." : "") + name);
                }
            }

            return outList;
        }

        public static List<string> ToMetadataNamesList()
        {
            return ToMetadataNamesList(null, false, new List<string>(), typeof(Metadata));
        }

        private static Dictionary<string, object> ToDictionary(
            string? parent,
            bool parentAdvanced,
            Dictionary<string, object> outDictionary,
            IMetadata entry)
        {
            var type = entry.GetType();
            foreach (var property in type.GetProperties())
            {
                var name = property.Name;
                var value = property.GetValue(entry);
                var advanced = parentAdvanced || (property.GetCustomAttribute<MetadataAdvancedAttribute>()?.Advanced ?? false);

                if (value == null)
                {
                    continue;
                }

                if (value is IMetadata metadataEntry)
                {
                    ToDictionary(parent != null ? parent + "." + name : name, advanced, outDictionary, metadataEntry);
                }
                else if (value is string || value is DateTimeOffset || value.GetType().IsPrimitive)
                {
                    outDictionary[
                        (advanced ? "[Advanced] " : "") + (parent != null ? parent + "." : "") + name] = value;
                }
            }

            return outDictionary;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return ToDictionary(null, false, new Dictionary<string, object>(), this);
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool prettier)
        {
            return JsonSerializer.Serialize(
                ToDictionary(),
                new JsonSerializerOptions { WriteIndented = prettier, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
