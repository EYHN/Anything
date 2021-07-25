using System;

namespace Anything.Preview.Metadata.Schema
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MetadataAdvancedAttribute : Attribute
    {
        public MetadataAdvancedAttribute(bool advanced = true)
        {
            Advanced = advanced;
        }

        public bool Advanced { get; }
    }
}
