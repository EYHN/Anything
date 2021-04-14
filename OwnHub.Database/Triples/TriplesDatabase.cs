using System;
using OwnHub.Database.Orm;

namespace OwnHub.Database.Triples
{
    public class TriplesDatabase : OrmSystem
    {
        private TriplesDatabase(OrmDatabaseProvider databaseProvider)
            : base(databaseProvider)
        {
        }

        public static TriplesDatabase Create(SqliteContext context, string tableName)
        {
            var databaseProvider = new TriplesDatabaseProvider(context, tableName);
            var database = new TriplesDatabase(databaseProvider);
            database.RegisteredScalar(typeof(bool));
            database.RegisteredScalar(typeof(byte));
            database.RegisteredScalar(typeof(byte[]));
            database.RegisteredScalar(typeof(char));
            database.RegisteredScalar(typeof(DateTime));
            database.RegisteredScalar(typeof(DateTimeOffset));
            database.RegisteredScalar(typeof(decimal));
            database.RegisteredScalar(typeof(double));
            database.RegisteredScalar(typeof(float));
            database.RegisteredScalar(typeof(Guid));
            database.RegisteredScalar(typeof(int));
            database.RegisteredScalar(typeof(long));
            database.RegisteredScalar(typeof(sbyte));
            database.RegisteredScalar(typeof(short));
            database.RegisteredScalar(typeof(string));
            database.RegisteredScalar(typeof(TimeSpan));
            database.RegisteredScalar(typeof(uint));
            database.RegisteredScalar(typeof(ulong));
            database.RegisteredScalar(typeof(ushort));
            database.RegisteredType(typeof(TriplesRoot));

            using var createTransaction = database.StartTransaction(ITransaction.TransactionMode.Create);
            createTransaction.CreateDatabase();
            var root = new TriplesRoot();
            createTransaction.Save(root);
            createTransaction.Commit();

            return database;
        }

        public new TriplesTransaction StartTransaction(ITransaction.TransactionMode mode, Type? customTransactionType = null)
        {
            return (TriplesTransaction)base.StartTransaction(mode, customTransactionType ?? typeof(TriplesTransaction));
        }
    }
}
