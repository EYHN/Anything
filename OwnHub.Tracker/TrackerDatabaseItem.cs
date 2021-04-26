using System;
using OwnHub.Database.Orm;
using OwnHub.Database.Triples;

namespace OwnHub.Tracker
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
