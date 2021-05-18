using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StagingBox.Utils;

namespace StagingBox.Tests.Utils
{
    [TestFixture]
    public class ObjectPoolTests
    {
        private class TestObject
        {
        }

        [Test]
        [Timeout(3000)]
        public async Task CreatorTest()
        {
            var createCount = 0;
            var pool = new ObjectPool<TestObject>(
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
            var pool = new ObjectPool<TestObject>(
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
            var pool = new ObjectPool<TestObject>(3, () => { return new TestObject(); });

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
                            await Task.Delay(40);
                            Assert.IsFalse(flag);
                        } // relese once

                        await Task.Delay(20);
                        Assert.IsTrue(flag);
                    }
                });

            var taskB = Task.Run(
                async () =>
                {
                    await Task.Delay(20);

                    // should block here
                    using (await pool.GetRefAsync())
                    {
                        flag = true;
                    }
                });

            await Task.WhenAll(taskA, taskB);
        }

        [Test]
        [Timeout(3000)]
        public async Task DisposeTest()
        {
            var pool = new ObjectPool<TestObject>(1, () => new TestObject());
            var thrown = false;

            var taskA = Task.Run(
                async () =>
                {
                    using (await pool.GetRefAsync())
                    {
                        try
                        {
                            // should block here
                            using var p = await pool.GetRefAsync();
                            Assert.Fail("Should not be run.");
                        }
                        catch (OperationCanceledException)
                        {
                            thrown = true;
                        }
                    }
                });

            var taskB = Task.Run(
                async () =>
                {
                    await Task.Delay(50);
                    pool.Dispose();
                    await Task.Delay(50);
                    Assert.IsTrue(thrown);
                });

            await Task.WhenAll(taskA, taskB);
        }

        [Test]
        [Timeout(3000)]
        public async Task DisposeAsyncCreatorTest()
        {
            var pool = new ObjectPool<TestObject>(
                2,
                async () =>
                {
                    await Task.Delay(100);
                    return new TestObject();
                });

            var thrown = false;

            var taskA = Task.Run(
                async () =>
                {
                    using (await pool.GetRefAsync())
                    {
                    }
                });

            var taskB = Task.Run(
                async () =>
                {
                    await Task.Delay(10);
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
                    }
                });

            var taskC = Task.Run(
                async () =>
                {
                    await Task.Delay(50);
                    pool.Dispose();
                    await Task.Delay(100);
                    Assert.IsTrue(thrown);
                });

            await Task.WhenAll(taskA, taskB, taskC);
        }
    }
}
