using Anything.Database;
using Anything.Database.Orm;
using Anything.Database.Triples;

namespace Anything.Tracker
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
