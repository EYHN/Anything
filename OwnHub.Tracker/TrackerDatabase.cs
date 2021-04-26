using System;
using OwnHub.Database;
using OwnHub.Database.Orm;
using OwnHub.Database.Triples;

namespace OwnHub.Tracker
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
