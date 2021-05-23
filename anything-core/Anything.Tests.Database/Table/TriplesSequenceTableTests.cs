using System.Threading.Tasks;
using Anything.Database;
using Anything.Database.Table;
using NUnit.Framework;

namespace Anything.Tests.Database.Table
{
    public class TriplesSequenceTableTests
    {
        [Test]
        public async Task FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var transaction = new SqliteTransaction(context, ITransaction.TransactionMode.Create);

            var table = new SequenceTable("TriplesSequenceTable");
            await table.CreateAsync(transaction);

            await table.InsertAsync(transaction, "Object");

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
