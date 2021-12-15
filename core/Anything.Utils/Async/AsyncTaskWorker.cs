using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Anything.Utils.Async;

/// <summary>
///     Execute async tasks in the background, limited number of tasks executed at the same time.
/// </summary>
public class AsyncTaskWorker
{
    private readonly Channel<int> _channel;

    public AsyncTaskWorker(int maxConcurrency)
    {
        _channel = Channel.CreateBounded<int>(maxConcurrency);
    }

    public int CurrentConcurrency { get; private set; }

    /// <summary>
    ///     Execute async functions in the background. If the number of tasks in execution exceeds the limit, the task will be suspended.
    ///     Return a ValueTask. It will be completed when the task starts to execute. if the task is suspended, the ValueTask is not completed.
    /// </summary>
    /// <returns>
    ///     Return a ValueTask. It will be completed when the task starts to execute. if the task is suspended, the ValueTask is not
    ///     completed.
    /// </returns>
    /// <param name="item">The function to run.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the run.</param>
    public async ValueTask Run(Func<Task> item, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(1, cancellationToken);
        CurrentConcurrency++;
        var task = Task.Factory.StartNew(item, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        _ = task.Unwrap().ContinueWith(
            t =>
            {
                if (_channel.Reader.TryRead(out var _))
                {
                    CurrentConcurrency--;
                }

                if (t.IsFaulted)
                {
                    Console.WriteLine(t.Exception);
                }
            },
            default,
            TaskContinuationOptions.None,
            TaskScheduler.Current);
    }
}
