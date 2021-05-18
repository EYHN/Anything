using System;
using System.Data;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using StagingBox.Database;
using StagingBox.Database.Orm;
using StagingBox.Database.Triples;

namespace StagingBox.Tests.Database.Triples
{
    public class TriplesDatabaseTests
    {
        [OrmType]
        public class SimpleCustomClass
        {
            [OrmProperty]
            public int A { get; set; }

#pragma warning disable SA1401
            [OrmProperty]
            public int B;
#pragma warning restore SA1401

            public int Sum()
            {
                return A + B;
            }
        }

        [OrmType]
        public class CustomClassWithCtor
        {
            [OrmConstructor]
            public CustomClassWithCtor([OrmProperty(nameof(A))] int a, [OrmProperty(nameof(B))] int b)
            {
                A = a;
                B = b;
            }

            [OrmProperty]
            public int A { get; }

#pragma warning disable SA1401
            [OrmProperty]
            public readonly int B;
#pragma warning restore SA1401

            public int Sum()
            {
                return A + B;
            }
        }

        [Test]
        public void SimpleCustomClassTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var db = TriplesDatabase.Create(context, "triples");
            db.RegisteredType(typeof(SimpleCustomClass));

            using (var t = db.StartTransaction(ITransaction.TransactionMode.Mutation))
            {
                var customObj1 = new SimpleCustomClass { A = 10, B = 20 };

                t.Root["obj1"] = customObj1;
                t.Save(t.Root);

                t.Commit();
            }

            using (var t = db.StartTransaction(ITransaction.TransactionMode.Query))
            {
                var customObj1 = t.Root["obj1"] as SimpleCustomClass;

                Assert.AreEqual(30, customObj1?.Sum());
            }
        }

        [Test]
        public void CustomClassWithCtorTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var db = TriplesDatabase.Create(context, "triples");
            db.RegisteredType(typeof(CustomClassWithCtor));

            using (var t = db.StartTransaction(ITransaction.TransactionMode.Mutation))
            {
                var customObj1 = new CustomClassWithCtor(10, 20);

                t.Root["obj1"] = customObj1;
                t.Save(t.Root);

                t.Commit();
            }

            using (var t = db.StartTransaction(ITransaction.TransactionMode.Query))
            {
                var customObj1 = t.Root["obj1"] as CustomClassWithCtor;

                Assert.AreEqual(30, customObj1?.Sum());
            }
        }

        [Test]
        public void IsolationTest()
        {
            var context = TestUtils.CreateSqliteContext();
            var db = TriplesDatabase.Create(context, "triples");

            using var writeTransaction = db.StartTransaction(ITransaction.TransactionMode.Mutation);

            writeTransaction.Root["obj1"] = "123";
            writeTransaction.Save(writeTransaction.Root);


            using var readTransaction1 = db.StartTransaction(ITransaction.TransactionMode.Query);
            Assert.AreEqual(null, readTransaction1.Root["obj1"]);

            writeTransaction.Commit();

            using var readTransaction2 = db.StartTransaction(ITransaction.TransactionMode.Query);
            Assert.AreEqual("123", readTransaction2.Root["obj1"]);
        }
    }
}
