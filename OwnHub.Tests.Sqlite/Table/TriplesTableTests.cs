using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class TriplesTableTests
    {
        [Test]
        public async Task FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var transaction = new SqliteTransaction(context, ITransaction.TransactionMode.Create);
            var table = new TriplesTable("TriplesTable");
            await table.CreateAsync(transaction);

            await table.InsertAsync(transaction, 0, "/image.jpg", 1, "O(File)");
            await table.InsertAsync(transaction, 0, "/audio.mp3", 2, "O(File)");
            await table.InsertAsync(transaction, 1, "Width", 100, "V(Long)");
            await table.InsertAsync(transaction, 1, "Height", 150, "V(Long)");
            await table.InsertAsync(transaction, 2, "album", "Tell your world", "V(String)");

            var result = await table.SelectAsync(transaction, 0, "/image.jpg", reader =>
            {
                reader.Read();
                Assert.AreEqual(reader["Object"], 1);
                Assert.AreEqual(reader["ObjectType"], "O(File)");
                return 123321;
            });
            Assert.AreEqual(result, 123321);

            await table.SelectAsync(transaction, 1, "Width", reader =>
            {
                reader.Read();
                Assert.AreEqual(reader["Object"], 100);
                Assert.AreEqual(reader["ObjectType"], "V(Long)");
                return 0;
            });
        }
    }
}
