using System;
using StagingBox.Database;
using StagingBox.Database.Orm;
using StagingBox.Database.Triples;

namespace StagingBox.Tracker
{
    public class TrackerDatabase
        : TriplesDatabase
    {
        protected TrackerDatabase(OrmDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        public new TrackerDatabaseTransaction StartTransaction(ITransaction.TransactionMode mode, Type? customTransactionType = null)
        {
            return (TrackerDatabaseTransaction)base.StartTransaction(mode, customTransactionType ?? typeof(TrackerDatabaseTransaction));
        }
    }
}
