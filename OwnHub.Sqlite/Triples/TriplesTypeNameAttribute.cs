using System;

namespace OwnHub.Sqlite.Triples
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TriplesTypeNameAttribute : Attribute
    {
        public TriplesTypeNameAttribute(string typeName)
        {
            TypeName = typeName;
        }

        internal string TypeName { get; }
    }
}