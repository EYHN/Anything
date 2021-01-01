using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Utils;

namespace OwnHub.Tests.Utils
{
    [TestFixture]
    public class AsyncTaskWorkerTests
    {
        [Test]
        [Timeout(3000)]
        public async Task RunTest()
        {
            int number = 0;
            Func<Task> asyncFunc = async () =>
            {
                number++;
                await Task.Delay(10);
                number++;
            };
            var worker = new AsyncTaskWorker(3);
            await worker.Run(asyncFunc);
            await worker.Run(asyncFunc);
            await worker.Run(asyncFunc);
            await worker.Run(asyncFunc);
            await worker.Run(asyncFunc);
            await Task.Delay(50);
            Assert.AreEqual(number, 10);
        }
        
        [Test]
        [Timeout(3000)]
        public async Task MaxConcurrencyTest()
        {
            Defer defer = new Defer();
            Defer endDefer = new Defer();
            
            var worker = new AsyncTaskWorker(3);
            // Three blocked tasks
            await worker.Run(() => defer.Wait());
            await worker.Run(() => defer.Wait());
            await worker.Run(() => defer.Wait());
            // Task will be suspended
            ValueTask step4Task = worker.Run(() =>
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
            bool flag = false;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            
            var worker = new AsyncTaskWorker(1);
            // blocked task
            await worker.Run(() => Task.Delay(1000, cancellationToken));
            // Task will be suspended
            ValueTask suspendedTask = worker.Run(() =>
            {
                flag = true;
                return Task.CompletedTask;
            }, cancellationToken);
            
            Assert.IsFalse(suspendedTask.IsCompleted);
            Assert.IsFalse(flag);
            // Cancel pending suspension
            cancellationTokenSource.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(() => suspendedTask.AsTask());
            // Wait for step4 end
            Assert.IsTrue(suspendedTask.IsCanceled);
            Assert.IsFalse(flag);
        }
    }
}