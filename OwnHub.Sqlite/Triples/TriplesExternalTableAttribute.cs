using System;

namespace OwnHub.Sqlite.Triples
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TriplesExternalTableAttribute : Attribute
    {
        public TriplesExternalTableAttribute(Type tableType, string tableName)
        {
            if (!tableType.IsAssignableTo(typeof(Table.Table)))
            {
                throw new ArgumentException("Must be a subclass of \"Table\".", nameof(tableType));
            }

            TableType = tableType;
            TableName = tableName;
        }

        internal Type TableType { get; }

        internal string TableName { get; }
    }
}
