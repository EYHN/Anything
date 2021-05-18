using System;
using NUnit.Framework;
using StagingBox.Database.Orm;

namespace StagingBox.Tests.Database.Orm
{
    public class OrmTypeTests
    {
        public class TestClass
        {
            [OrmProperty]
            public int A { get; set; }

            [OrmProperty]
            public long B { get; set; }
        }

        public class TestClassWithCtor
        {
            [OrmProperty]
            public int A { get; set; }

            [OrmProperty]
            public long B { get; }

            [OrmConstructor]
            public TestClassWithCtor([OrmProperty("A")] int a, [OrmProperty("B")] long b)
            {
                A = a;
                B = b;
            }
        }

        [Test]
        public void TypeAnalyzeTest()
        {
            var typeManager = new OrmTypeManager();
            var scalarInfo1 = typeManager.RegisterScalar(typeof(int));
            var scalarInfo2 = typeManager.RegisterScalar(typeof(long));
            var typeInfo1 = typeManager.RegisterType(typeof(TestClass));

            Assert.AreEqual(2, typeInfo1.Properties.Length);
            Assert.AreEqual(scalarInfo1, typeInfo1.GetProperty("A")?.ValueTypeInfo);
            Assert.AreEqual(scalarInfo2, typeInfo1.GetProperty("B")?.ValueTypeInfo);

            var typeInfo2 = typeManager.RegisterType(typeof(TestClassWithCtor));
            Assert.AreEqual(typeInfo2.GetProperty("A"), typeInfo2.Constructor.Parameters[0].BindProperty);
            Assert.AreEqual(typeInfo2.GetProperty("B"), typeInfo2.Constructor.Parameters[1].BindProperty);
        }

        [Test]
        public void TypeDependencyErrorTest()
        {
            var typeManager = new OrmTypeManager();
            typeManager.RegisterScalar(typeof(int));

            Assert.Catch<InvalidOperationException>(() => typeManager.RegisterType(typeof(TestClass)));
        }
    }
}
