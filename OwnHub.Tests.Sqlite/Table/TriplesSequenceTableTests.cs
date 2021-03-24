using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class TriplesSequenceTableTests
    {
        [Test]
        public async Task FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext("FeatureTest");
            var connection = context.GetCreateConnectionRef().Value;
            var table = new TriplesSequenceTable("TriplesSequenceTable");
            await table.CreateAsync(connection);

            await table.InsertAsync(connection, "Object", 0);

            // ignore if exist
            await table.InsertAsync(connection, "Object", 0, true);
            await table.InsertAsync(connection, "External", 0, true);
            await table.InsertAsync(connection, "External", 0, true);

            Assert.AreEqual(await table.IncreaseSeqAsync(connection, "Object"), 1);
            Assert.AreEqual(await table.IncreaseSeqAsync(connection, "Object"), 2);
            Assert.AreEqual(await table.IncreaseSeqAsync(connection, "Object"), 3);

            Assert.AreEqual(await table.IncreaseSeqAsync(connection, "External"), 1);
            Assert.AreEqual(await table.IncreaseSeqAsync(connection, "External"), 2);
            Assert.AreEqual(await table.IncreaseSeqAsync(connection, "External"), 3);
        }
    }
}
