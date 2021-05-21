using System;
using Anything.Database;
using Anything.Database.Orm;
using Anything.Database.Triples;

namespace Anything.Tracker
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
