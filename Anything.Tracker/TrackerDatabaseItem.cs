using System;
using Anything.Database.Orm;
using Anything.Database.Triples;

namespace Anything.Tracker
{
    public class TrackerDatabaseItem
        : TriplesBaseObject
    {
        [OrmProperty]
        public DateTimeOffset VerificationTime { get; set; }

        public void AddFork(string name, object fork)
        {
            if (this[name] != null)
            {
                throw new InvalidOperationException($"Fork \"{name}\" already exists.");
            }
        }
    }
}
