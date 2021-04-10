using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class TriplesSequenceTableTests
    {
        [Test]
        public async Task FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var transaction = new SqliteTransaction(context, ITransaction.TransactionMode.Create);

            var table = new TriplesSequenceTable("TriplesSequenceTable");
            await table.CreateAsync(transaction);

            await table.InsertAsync(transaction, "Object", 0);

            // ignore if exist
            await table.InsertAsync(transaction, "Object", 0, true);
            await table.InsertAsync(transaction, "External", 0, true);
            await table.InsertAsync(transaction, "External", 0, true);

            Assert.AreEqual(await table.IncreaseSeqAsync(transaction, "Object"), 1);
            Assert.AreEqual(await table.IncreaseSeqAsync(transaction, "Object"), 2);
            Assert.AreEqual(await table.IncreaseSeqAsync(transaction, "Object"), 3);

            Assert.AreEqual(await table.IncreaseSeqAsync(transaction, "External"), 1);
            Assert.AreEqual(await table.IncreaseSeqAsync(transaction, "External"), 2);
            Assert.AreEqual(await table.IncreaseSeqAsync(transaction, "External"), 3);
        }
    }
}
