using Anything.Database.Orm;

namespace Anything.Database.Triples
{
    /// <summary>
    /// Represents the root object in the triples database.
    /// </summary>
    [OrmType("Root")]
    public class TriplesRoot : TriplesBaseObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriplesRoot"/> class.
        /// </summary>
        public TriplesRoot()
        {
        }

        public void Set(string key, object? value)
        {
            this[key] = value;
        }

        public void SetAndSave(string key, object? value)
        {
            this[key] = value;
            Transaction.Save(this);
        }
    }
}
