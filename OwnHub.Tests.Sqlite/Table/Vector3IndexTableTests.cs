using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Table;

namespace OwnHub.Tests.Sqlite.Table
{
    public class Vector3IndexTableTests
    {
        public SqliteContext CreateSqliteContext(string name)
        {
            return new SqliteContext(new SharedMemoryConnectionProvider("Vector3IndexTableTests-" + name));
        }
        
        [Test]
        public async Task FeatureTest()
        {
            SqliteContext context = CreateSqliteContext("FeatureTest");
            Vector3IndexTable table = new Vector3IndexTable(context, "Vector3IndexTable");
            await table.Create();

            await table.Insert("1", new Vector3(1,2,3));
            await table.Insert("2", new Vector3(11,22,33));
            await table.Insert("3", new Vector3(111,222,333));

            Vector3IndexTable.Row[] searchResult = (await table.Search(new Vector3(10, 10, 10), new Vector3(100, 100, 100))).ToArray();
            
            Assert.IsTrue(searchResult.Length == 1);
            Assert.AreEqual(searchResult[0].Id, "2");

            await table.Delete("2");
            
            searchResult = (await table.Search(new Vector3(10, 10, 10), new Vector3(100, 100, 100))).ToArray();
            Assert.IsTrue(searchResult.Length == 0);
        }

        [Test]
        public async Task ExtraDataTest()
        {
            SqliteContext context = CreateSqliteContext("FeatureTest");
            Vector3IndexTable table = new Vector3IndexTable(context, "Vector3IndexTable");
            await table.Create();
            
            await table.Insert("1", new Vector3(1,2,3));
            string extraData = "any string data";
            await table.Insert("2", new Vector3(11,22,33), extraData);
            
            Vector3IndexTable.Row[] searchResult = (await table.Search(new Vector3(10, 10, 10), new Vector3(100, 100, 100))).ToArray();
            Assert.IsTrue(searchResult.Length == 1);
            Assert.AreEqual(searchResult[0].ExtraData, extraData);
            
            searchResult = (await table.Search(new Vector3(0, 0, 0), new Vector3(10, 10, 10))).ToArray();
            Assert.IsTrue(searchResult.Length == 1);
            Assert.AreEqual(searchResult[0].ExtraData, null);
        }
    }
}