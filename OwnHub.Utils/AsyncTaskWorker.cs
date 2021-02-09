﻿using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OwnHub.Utils
{
    /// <summary>
    /// Execute async tasks in the background, limited number of tasks executed at the same time.
    /// </summary>
    public class AsyncTaskWorker
    {
        private readonly Channel<int> channel;
        public int CurrentConcurrency { get; private set; } = 0;

        public AsyncTaskWorker(int maxConcurrency)
        {
            channel = Channel.CreateBounded<int>(maxConcurrency);
        }

        /// <summary>
        /// Execute async functions in the background. If the number of tasks in execution exceeds the limit, the task will be suspended.
        ///
        /// Return a ValueTask. It will be completed when the task starts to execute. if the task is suspended, the ValueTask is not completed.
        /// </summary>
        /// <returns>Return a ValueTask. It will be completed when the task starts to execute. if the task is suspended, the ValueTask is not completed.</returns>
        public async ValueTask Run(Func<Task> item, CancellationToken cancellationToken = default)
        {
            await channel.Writer.WriteAsync(1, cancellationToken);
            CurrentConcurrency++;
            Task task = Task.Run(item, cancellationToken);
            _ = task.ContinueWith(t =>
            {
                if (channel.Reader.TryRead(out int _))
                {
                    CurrentConcurrency--;
                }
                if (t.IsFaulted)
                {
                    Console.WriteLine(t.Exception);
                }
            }, default (CancellationToken));
        }
    }
}