using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using OwnHub.Sqlite;
using OwnHub.Sqlite.Provider;
using OwnHub.Sqlite.Triples;

namespace OwnHub.Tests.Sqlite.Triples
{
    public class TriplesDatabaseTests
    {
        [Test]
        public void FeatureTest()
        {
            var context = TestUtils.CreateSqliteContext("FeatureTest");
            var database = new TriplesDatabase(context, "Triples");
            database.RegisterObjectType<TestObject>();
            database.Create();

            // Database root can set and get child.
            database.Root.SetChild("hello", "world");
            database.Root.TryGetChild("hello", out string? value);
            Assert.AreEqual("world", value);

            // Custom object type.
            TestObject testObject = new() { Name = "test name", Number = 1 };

            // The properties set when the object is not connected to the database will be stored in the memory cache.
            var name = testObject.Name;
            var number = testObject.Number;
            Assert.AreEqual("test name", name);
            Assert.AreEqual(1, number);

            // Attach objects to root can be saved to the database.
            // When the object is saved to the database, all properties are also written to the database.
            database.Root.SetChild("test1", testObject);
            database.Root.TryGetChild("test1", out testObject!);
            Assert.AreEqual("test name", testObject.Name);

            // An object can only be attached once.
            Assert.Throws<InvalidOperationException>(() => database.Root.SetChild("test2", testObject));

            // The delete operation will release the object, the object cannot be operated after release.
            database.Root.DeleteChild("test1");
            Assert.Throws<SqliteException>(() => testObject.Number = 123);

            // An object can contain other objects.
            TestObject parentObject = new();
            TestObject childObject1 = new();
            TestObject childObject2 = new();
            TestObject childObject3 = new();
            childObject1.Name = "child1";
            childObject1.Child = childObject2;
            childObject2.Name = "child2";
            parentObject.Child = childObject1;

            database.Root.SetChild("test3", parentObject);

            childObject2.Child = childObject3;
            childObject3.Name = "child3";

            database.Root.TryGetChild("test3", out parentObject!);
            Assert.AreEqual("child1", parentObject.Child!.Name);
            Assert.AreEqual("child2", parentObject.Child!.Child!.Name);
            Assert.AreEqual("child3", parentObject.Child!.Child!.Child!.Name);

            // Delete the parent object will release all child objects.
            database.Root.DeleteChild("test3");
            Assert.Throws<SqliteException>(() => childObject3.Name = "123");
            Assert.AreEqual(null, database.Root.GetChild<TestObject>("test3"));
            Assert.AreEqual(null, database.Root.GetChild<int?>("test3"));
        }

        [Test]
        public async Task AsyncTest()
        {
            var context = TestUtils.CreateSqliteContext("AsyncTest");
            var database = new TriplesDatabase(context, "Triples");
            database.RegisterObjectType<TestObject>();
            await database.CreateAsync();

            await database.Root.SetChildAsync("hello", "world");
            var value = await database.Root.GetChildAsync<string?>("hello");
            Assert.AreEqual("world", value);

            TestObject testObject = new();
            await testObject.SetNameAsync("test name");
            await testObject.SetNumberAsync(1);

            await database.Root.SetChildAsync("test1", testObject);
            testObject = (await database.Root.GetChildAsync<TestObject>("test1"))!;
            Assert.AreEqual("test name", await testObject.GetNameAsync());
            Assert.AreEqual(1, await testObject.GetNumberAsync());

            await database.Root.DeleteChildAsync("test1");
        }

        [Test]
        public void RegistryObjectTypeTest()
        {
            var context = TestUtils.CreateSqliteContext("RegistryObjectTypeTest");
            var database = new TriplesDatabase(context, "Triples");

            Assert.Throws<ArgumentException>(
                () =>
                {
                    database.RegisterObjectType(typeof(object));
                });

            Assert.Throws<ArgumentException>(
                () =>
                {
                    database.RegisterObjectType(typeof(TriplesObject));
                });
        }

        [Test]
        public void ValueTypeTest()
        {
            var context = TestUtils.CreateSqliteContext("ValueTypeTest");
            var database = new TriplesDatabase(context, "Triples");
            database.Create();

            void Test<T>(string name, T value)
                where T : notnull
            {
                database.Root.SetChild(name, value);
                database.Root.TryGetChild(name, out object? result);

                Assert.AreEqual(typeof(T), result?.GetType());
                Assert.AreEqual(value, result);
            }

            Test("Bool", true);
            Test("Byte", byte.MaxValue);
            Test("Blob", new byte[] { 0, 1, 2 });
            Test("Char", char.MaxValue);
            Test("DateTime", DateTime.Now);
            Test("DateTimeOffset", DateTimeOffset.Now);
            Test("Decimal", decimal.MaxValue);
            Test("Double", double.MaxValue);
            Test("Float", float.MaxValue);
            Test("Guid", Guid.NewGuid());
            Test("Int32", int.MaxValue);
            Test("Int64", long.MaxValue);
            Test("Sbyte", sbyte.MaxValue);
            Test("Short", short.MaxValue);
            Test("String", "hello world");
            Test("TimeSpace", TimeSpan.MaxValue);
            Test("Uint", uint.MaxValue);
            Test("Ulong", ulong.MaxValue);
            Test("Ushort", ushort.MaxValue);

            Assert.Throws<ArgumentException>(
                () =>
                {
                    database.Root.SetChild("unsupported value", new object());
                });

            Assert.Throws<ArgumentException>(
                () =>
                {
                    database.Root.SetChild("unregistered object type", new TestObject());
                });
        }

        [TriplesTypeName("Test")]
        private class TestObject : TriplesObject
        {
            public string? Name
            {
                get
                {
                    TryGetProperty("name", out string? name);
                    return name;
                }
                set => SetProperty("name", value!);
            }

            public int? Number
            {
                get
                {
                    TryGetProperty("number", out int? number);
                    return number;
                }
                set => SetProperty("number", value!);
            }

            public TestObject? Child
            {
                get
                {
                    TryGetProperty("child", out TestObject? child);
                    return child;
                }
                set => SetProperty("child", value!);
            }

            public ValueTask SetNameAsync(string name) => SetPropertyAsync("name", name);

            public ValueTask<string?> GetNameAsync() => GetPropertyAsync<string?>("name");

            public ValueTask SetNumberAsync(int number) => SetPropertyAsync("number", number);

            public ValueTask<int?> GetNumberAsync() => GetPropertyAsync<int?>("number");

            public ValueTask SetChildAsync(TestObject child) => SetPropertyAsync("child", child);

            public ValueTask<TestObject?> GetChildAsync() => GetPropertyAsync<TestObject?>("child");
        }
    }
}
