using StagingBox.Database;
using StagingBox.Database.Orm;
using StagingBox.Database.Triples;

namespace StagingBox.Tracker
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
