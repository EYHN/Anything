using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Utils;
using OwnHub.Utils.Async;

namespace OwnHub.Tests.Utils
{
    [TestFixture]
    public class AsyncTaskWorkerTests
    {
        [Test]
        [Timeout(3000)]
        public async Task RunTest()
        {
            var defer = new Defer();
            var number = 0;

            async Task AsyncFunc()
            {
                number++;
                await Task.Delay(10);
                number++;
                if (number == 10)
                {
                    defer.Resolve();
                }
            }

            var worker = new AsyncTaskWorker(3);
            await worker.Run(AsyncFunc);
            await worker.Run(AsyncFunc);
            await worker.Run(AsyncFunc);
            await worker.Run(AsyncFunc);
            await worker.Run(AsyncFunc);
            await defer.Wait();
            Assert.AreEqual(number, 10);
        }

        [Test]
        [Timeout(3000)]
        public async Task MaxConcurrencyTest()
        {
            var defer = new Defer();
            var endDefer = new Defer();

            var worker = new AsyncTaskWorker(3);

            // Three blocked tasks
            await worker.Run(() => defer.Wait());
            await worker.Run(() => defer.Wait());
            await worker.Run(() => defer.Wait());

            // Task will be suspended
            var step4Task = worker.Run(
                () =>
                {
                    endDefer.Resolve();
                    return Task.CompletedTask;
                });

            Assert.IsFalse(step4Task.IsCompleted);
            Assert.IsFalse(endDefer.IsCompleted);

            // End the blocked tasks
            defer.Resolve();

            // Wait for step4 end
            await step4Task;
            await endDefer.Wait();
            Assert.IsTrue(step4Task.IsCompleted);
        }

        [Test]
        [Timeout(3000)]
        public async Task CancelTest()
        {
            var flag = false;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var worker = new AsyncTaskWorker(1);

            // blocked task
            var blockedDefer = new Defer();
            await worker.Run(() => blockedDefer.Wait());

            // Task will be suspended
            var suspendedTask = worker.Run(
                () =>
                {
                    flag = true;
                    return Task.CompletedTask;
                },
                cancellationToken);

            Assert.IsFalse(suspendedTask.IsCompleted);
            Assert.IsFalse(flag);

            // Cancel pending suspension
            cancellationTokenSource.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(() => suspendedTask.AsTask());

            Assert.IsTrue(suspendedTask.IsCanceled);
            Assert.IsFalse(flag);
            blockedDefer.Resolve();
        }
    }
}
