using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class TriplesTableTests
    {
        private static SqliteContext CreateSqliteContext(string name)
        {
            return new (new SharedMemoryConnectionProvider("TriplesTableTests-" + name));
        }

        [Test]
        public async Task FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext("FeatureTest");
            var connection = context.GetCreateConnectionRef().Value;
            var table = new TriplesTable("TriplesTable");
            await table.CreateAsync(connection);

            var transaction = new SqliteTransaction(context, SqliteTransaction.TransactionMode.Mutation);
            await table.InsertAsync(transaction, 0, "/image.jpg", 1, "Object(File)");
            await table.InsertAsync(transaction, 0, "/audio.mp3", 2, "Object(File)");
            await table.InsertAsync(transaction, 1, "Width", 100, "Value(Long)");
            await table.InsertAsync(transaction, 1, "Height", 150, "Value(Long)");
            await table.InsertAsync(transaction, 2, "album", "Tell your world", "Value(String)");

            var result = await table.SelectAsync(transaction, 0, "/image.jpg", reader =>
            {
                reader.Read();
                Assert.AreEqual(reader["Object"], 1);
                Assert.AreEqual(reader["ObjectType"], "Object(File)");
                return 123321;
            });
            Assert.AreEqual(result, 123321);

            await table.SelectAsync(transaction, 1, "Width", reader =>
            {
                reader.Read();
                Assert.AreEqual(reader["Object"], 100);
                Assert.AreEqual(reader["ObjectType"], "Value(Long)");
                return 0;
            });
        }
    }
}
