using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Utils;

namespace OwnHub.Sqlite.Triples
{
    public sealed partial class TriplesTransaction : SqliteTransaction
    {
        public TriplesDatabase Database { get; }

        public TriplesTransaction(
            TriplesDatabase database,
            TransactionMode mode)
            : base(database.SqliteContext, mode)
        {
            Database = database;
        }
    }
}
