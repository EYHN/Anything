using System.IO;
using Anything.Database;
using Anything.Database.Provider;
using NUnit.Framework;

namespace Anything.Tests.Database
{
    public class SqliteContextTests
    {
        [Test]
        public void BasicQueryTest()
        {
            var dir = TestUtils.GetTestDirectoryPath();
            var sqliteContext = new SqliteContext(new SqliteConnectionProvider(Path.Join(dir, "test.db")));

            using (var transaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Create))
            {
                transaction.ExecuteNonQuery(() => @"CREATE TABLE IF NOT EXISTS TestTable (Name TEXT, Data Text);", "create");
                transaction.Commit();
            }

            using (var transaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Mutation))
            {
                transaction.ExecuteNonQuery(() => @"INSERT INTO TestTable (Name, Data) VALUES('foo', 'bar');", "mutation");
                transaction.Commit();
            }

            using (var transaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Query))
            {
                var result = transaction.ExecuteScalar(() => @"SELECT Data FROM TestTable WHERE Name='foo';", "query");
                Assert.AreEqual("bar", result);
            }
        }

        [Test]
        public void TransactionIsolationTest()
        {
            var dir = TestUtils.GetTestDirectoryPath();
            var sqliteContext = new SqliteContext(new SqliteConnectionProvider(Path.Join(dir, "test.db")));

            using (var transaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Create))
            {
                transaction.ExecuteNonQuery(() => @"CREATE TABLE IF NOT EXISTS TestTable (Name TEXT, Data Text);", "create");
                transaction.Commit();
            }

            using (var transaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Mutation))
            {
                transaction.ExecuteNonQuery(() => @"INSERT INTO TestTable (Name, Data) VALUES('foo', 'bar');", "mutation");

                using (var queryTransaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Query, true))
                {
                    var result = queryTransaction.ExecuteScalar(() => @"SELECT Data FROM TestTable WHERE Name='foo';", "query");
                    Assert.AreEqual(null, result);
                }

                transaction.Commit();

                using (var queryTransaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Query, true))
                {
                    var result = queryTransaction.ExecuteScalar(() => @"SELECT Data FROM TestTable WHERE Name='foo';", "query");
                    Assert.AreEqual("bar", result);
                }
            }
        }

        [Test]
        public void BusyErrorTest()
        {
            var dir = TestUtils.GetTestDirectoryPath();
            var sqliteContext = new SqliteContext(new SqliteConnectionProvider(Path.Join(dir, "test.db")));

            var transaction = new SqliteTransaction(sqliteContext, ITransaction.TransactionMode.Create);

            transaction.ExecuteNonQuery(() => @"CREATE TABLE IF NOT EXISTS TestTable (Name TEXT, Data Text);", "create");

            transaction.ExecuteNonQuery(() => @"INSERT INTO TestTable (Name, Data) VALUES('foo', 'bar');", "mutation");

            transaction.ExecuteNonQuery(() => @"INSERT INTO TestTable (Name, Data) VALUES('foo', 'bar');", "mutation");

            Assert.Catch(
                () => transaction.ExecuteReader(() => @"SELECT Data FROM TestTable WHERE Name='foo';", "query-foo-data", _ =>
                {
                    transaction.ExecuteNonQuery(() => @"INSERT INTO TestTable (Name, Data) VALUES('foo', 'bar');", "mutation");
                    return 0;
                }));

            var result = transaction.ExecuteScalar(() => @"SELECT Data FROM TestTable WHERE Name='foo';", "query-foo-data");
            Assert.AreEqual("bar", result);
        }
    }
}
