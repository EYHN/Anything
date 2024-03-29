using System;

namespace Anything.Preview.Meta.Schema;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MetadataAdvancedAttribute : Attribute
{
    public MetadataAdvancedAttribute(bool advanced = true)
    {
        Advanced = advanced;
    }

    public bool Advanced { get; }
}
