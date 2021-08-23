using System;
using System.Threading.Tasks;
using Anything.Utils;
using Anything.Utils.Async;
using NUnit.Framework;

namespace Anything.Tests.Utils
{
    [TestFixture]
    public class ObjectPoolTests
    {
        private class TestObject
        {
        }

        [Test]
        [Timeout(3000)]
        public void SyncTest()
        {
            var createCount = 0;
            using var pool = new ObjectPool<TestObject>(
                3,
                async () =>
                {
                    await Task.Delay(10);
                    createCount++;
                    return new TestObject();
                });

            var a = pool.Get();
            pool.Get();
            pool.Get();
            Assert.AreEqual(createCount, 3);

            Assert.AreEqual(null, pool.Get(false));
            Assert.AreEqual(createCount, 3);

            pool.Return(a);
            Assert.AreEqual(a, pool.Get());
        }

        [Test]
        [Timeout(3000)]
        public async Task CreatorTest()
        {
            var createCount = 0;
            using var pool = new ObjectPool<TestObject>(
                3,
                () =>
                {
                    createCount++;
                    return new TestObject();
                });

            using (await pool.GetRefAsync())
            using (await pool.GetRefAsync())
            using (await pool.GetRefAsync())
            {
                Assert.AreEqual(createCount, 3);
            }
        }

        [Test]
        [Timeout(3000)]
        public async Task AsyncCreatorTask()
        {
            var createCount = 0;
            using var pool = new ObjectPool<TestObject>(
                3,
                async () =>
                {
                    await Task.Delay(10);
                    createCount++;
                    return new TestObject();
                });

            using (await pool.GetRefAsync())
            using (await pool.GetRefAsync())
            using (await pool.GetRefAsync())
            {
                Assert.AreEqual(createCount, 3);
            }
        }

        [Test]
        [Timeout(3000)]
        public async Task BlockTest()
        {
            var step1defer = new Defer();
            var step2defer = new Defer();

            using var pool = new ObjectPool<TestObject>(3, () => { return new TestObject(); });

            var flag = false;

            var taskA = Task.Run(
                async () =>
                {
                    using (await pool.GetRefAsync())
                    using (await pool.GetRefAsync())
                    {
                        using (await pool.GetRefAsync())
                        {
                            // the pool is empty now
                            step1defer.Resolve();
                            await Task.Delay(50);
                            Assert.IsFalse(flag);
                        } // relese once

                        await step2defer.Wait();
                        Assert.IsTrue(flag);
                    }
                });

            var taskB = Task.Run(
                async () =>
                {
                    await step1defer.Wait();

                    // should block here
                    using (await pool.GetRefAsync())
                    {
                        flag = true;
                        step2defer.Resolve();
                    }
                });

            await Task.WhenAll(taskA, taskB);
        }

        [Test]
        [Timeout(3000)]
        public async Task DisposeTest()
        {
            var step1defer = new Defer();
            var step2defer = new Defer();

            using var pool = new ObjectPool<TestObject>(1, () => new TestObject());
            var thrown = false;

            var taskA = Task.Run(
                async () =>
                {
                    using (await pool.GetRefAsync())
                    {
                        step1defer.Resolve();
                        try
                        {
                            // should block here
                            using var p = await pool.GetRefAsync();
                            Assert.Fail("Should not be run.");
                        }
                        catch (OperationCanceledException)
                        {
                            thrown = true;
                            step2defer.Resolve();
                        }
                    }
                });

            var taskB = Task.Run(
                async () =>
                {
                    await step1defer.Wait();
                    pool.Dispose();
                    await step2defer.Wait();
                    Assert.IsTrue(thrown);
                });

            await Task.WhenAll(taskA, taskB);
        }

        [Test]
        [Timeout(3000)]
        public async Task DisposeAsyncCreatorTest()
        {
            var step1defer = new Defer();
            var step2defer = new Defer();

            using var pool = new ObjectPool<TestObject>(
                1,
                async () =>
                {
                    await Task.Delay(10);
                    return new TestObject();
                });

            var thrown = false;

            var taskA = Task.Run(
                async () =>
                {
                    using (await pool.GetRefAsync())
                    {
                        step1defer.Resolve();
                        try
                        {
                            using (await pool.GetRefAsync())
                            {
                                Assert.Fail("Should not be run.");
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            thrown = true;
                            step2defer.Resolve();
                        }
                    }
                });

            var taskB = Task.Run(
                async () =>
                {
                    await step1defer.Wait();
                    pool.Dispose();
                    await step2defer.Wait();
                    Assert.IsTrue(thrown);
                });

            await Task.WhenAll(taskA, taskB);
        }
    }
}
