using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Orm;
using OwnHub.Sqlite.Triples;

namespace OwnHub.Tests.Sqlite.Triples
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
    }
}
