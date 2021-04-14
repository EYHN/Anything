using System;

#pragma warning disable SA1402, SA1649

namespace OwnHub.Database.Orm
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class OrmPropertyAttribute : Attribute
    {
        public OrmPropertyAttribute(string? name = null)
        {
            Name = name;
        }

        internal string? Name { get; }
    }

    [AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
    public class OrmConstructorAttribute : Attribute
    {
        public OrmConstructorAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class OrmTypeAttribute : Attribute
    {
        public OrmTypeAttribute(string? name = null)
        {
            Name = name;
        }

        internal string? Name { get; }
    }
}
