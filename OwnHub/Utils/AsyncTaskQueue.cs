using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OwnHub.Utils
{
    public class AsyncTaskWorker
    {
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private readonly CancellationToken cancellationToken;
        private readonly Channel<Func<Task>> channel;
        private bool started = false;

        public AsyncTaskWorker(int maxQueueSize)
        {
            channel = Channel.CreateBounded<Func<Task>>(maxQueueSize);
            cancellationToken = cancellationSource.Token;
            
        }

        private async Task ExecLoop()
        {
            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                Func<Task> action = await channel.Reader.ReadAsync(cancellationToken);
                try
                {
                    await action.Invoke();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Async task queue uncaught exception!");
                    Console.WriteLine(e.ToString());
                }
            }
            
        }

        public async Task Run(Func<Task> item)
        {
            if (started == false)
            {
                started = true;
                _ = ExecLoop().ContinueWith(t =>
                {
                    Console.WriteLine(t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            await channel.Writer.WriteAsync(item, cancellationToken);
        }
    }
}
