using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Database;
using OwnHub.Database.Provider;
using OwnHub.Database.Table;

namespace OwnHub.Tests.Database.Table
{
    public class Vector3IndexTableTests
    {
        [Test]
        public async Task FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var transaction = new SqliteTransaction(context, ITransaction.TransactionMode.Create);
            var table = new Vector3IndexTable("Vector3IndexTable");
            await table.CreateAsync(transaction);

            await table.InsertAsync(transaction, 1, new Vector3(1, 2, 3));
            await table.InsertAsync(transaction, 2, new Vector3(11, 22, 33));
            await table.InsertAsync(transaction, 3, new Vector3(111, 222, 333));

            var searchResult = (await table.SearchAsync(transaction, new Vector3(10, 10, 10), new Vector3(100, 100, 100))).ToArray();

            Assert.IsTrue(searchResult.Length == 1);
            Assert.AreEqual(2, searchResult[0].Id);

            await table.DeleteAsync(transaction, 2);

            searchResult = (await table.SearchAsync(transaction, new Vector3(10, 10, 10), new Vector3(100, 100, 100))).ToArray();
            Assert.IsTrue(searchResult.Length == 0);
        }

        [Test]
        public async Task ExtraDataTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var transaction = new SqliteTransaction(context, ITransaction.TransactionMode.Create);
            var table = new Vector3IndexTable("Vector3IndexTable");
            await table.CreateAsync(transaction);

            await table.InsertAsync(transaction, 1, new Vector3(1, 2, 3));
            var extraData = "any string data";
            await table.InsertAsync(transaction, 2, new Vector3(11, 22, 33), extraData);

            var searchResult = (await table.SearchAsync(transaction, new Vector3(10, 10, 10), new Vector3(100, 100, 100))).ToArray();
            Assert.IsTrue(searchResult.Length == 1);
            Assert.AreEqual(extraData, searchResult[0].ExtraData);
            searchResult = (await table.SearchAsync(transaction, new Vector3(0, 0, 0), new Vector3(10, 10, 10))).ToArray();
            Assert.IsTrue(searchResult.Length == 1);
            Assert.AreEqual(null, searchResult[0].ExtraData);
        }
    }
}
