using OwnHub.Database;
using OwnHub.Database.Orm;
using OwnHub.Database.Triples;

namespace OwnHub.Tracker
{
    public class TrackerDatabaseTransaction
        : TriplesTransaction
    {
        public TrackerDatabaseTransaction(OrmSystem ormSystem, ITransaction.TransactionMode mode) : base(ormSystem, mode)
        {
        }

        private TrackerDatabaseRoot? _root;
        public new TrackerDatabaseRoot Root => _root ??= GetObjectOrDefault<TrackerDatabaseRoot>(0)!;
    }
}
