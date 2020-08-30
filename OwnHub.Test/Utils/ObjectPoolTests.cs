using OwnHub.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OwnHub.Utils.Tests
{
    [TestClass]
    public class ObjectPoolTests
    {

        class TestObject
        {

            public TestObject()
            {
            }
        }

        [TestMethod]
        [Timeout(3000)]
        public async Task ShouldCallCreator()
        {
            int createCount = 0;
            ObjectPool<TestObject> pool = new ObjectPool<TestObject>(3, () => { createCount++; return new TestObject(); });

            using (await pool.GetDisposableAsync())
            using (await pool.GetDisposableAsync())
            using (await pool.GetDisposableAsync())
            {
                Assert.AreEqual(createCount, 3);
            }
        }

        [TestMethod]
        [Timeout(3000)]
        public async Task ShouldBlockWhenPoolEmpty()
        {
            ObjectPool<TestObject> pool = new ObjectPool<TestObject>(3, () => { return new TestObject(); });

            bool flag = false;

            var taskA = Task.Run(async () =>
            {
                using (await pool.GetDisposableAsync())
                using (await pool.GetDisposableAsync())
                {
                    using (await pool.GetDisposableAsync())
                    {
                        // the pool is empty now
                        await Task.Delay(20);
                        Assert.IsFalse(flag);
                    } // relese once
                    await Task.Delay(10);
                    Assert.IsTrue(flag);
                }
            });

            var taskB = Task.Run(async () =>
            {
                await Task.Delay(10);
                // should block here
                using (await pool.GetDisposableAsync())
                {
                    flag = true;
                }
            });

            await Task.WhenAll(taskA, taskB);
        }
    }
}
