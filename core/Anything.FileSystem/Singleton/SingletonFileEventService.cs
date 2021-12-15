using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx.Synchronous;
using Nito.Disposables;

namespace Anything.FileSystem.Singleton;

public class SingletonFileEventService : SingleDisposable<object?>, IFileEventService
{
    private const int QueueSize = 100;
    private readonly Task<Task> _eventConsumerTask;
    private readonly Channel<FileEvent[]> _eventQueue = Channel.CreateBounded<FileEvent[]>(QueueSize);
    private readonly LinkedList<Func<IEnumerable<FileEvent>, ValueTask>> _listeners = new();
    private bool _eventConsumerBusy;

    public SingletonFileEventService(IServiceProvider serviceProvider)
        : base(null)
    {
        _eventConsumerTask = Task.Factory.StartNew(
            async () =>
            {
                while (await _eventQueue.Reader.WaitToReadAsync())
                {
                    try
                    {
                        _eventConsumerBusy = true;

                        List<FileEvent> events = new();
                        while (_eventQueue.Reader.TryRead(out var nextEvent))
                        {
                            events.AddRange(nextEvent);
                        }

                        if (events.Count == 0)
                        {
                            continue;
                        }

                        await using var scope = serviceProvider.CreateAsyncScope();
                        var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<IFileEventHandler>>();
                        var listeners = _listeners.Concat(handlers.Select(h => (Func<IEnumerable<FileEvent>, ValueTask>)h.OnFileEvent));
                        foreach (var listener in listeners)
                        {
                            try
                            {
                                await listener(events);
                            }
                            catch (System.Exception ex)
                            {
                                await Console.Error.WriteLineAsync(ex.ToString());
                            }
                        }
                    }
                    finally
                    {
                        _eventConsumerBusy = false;
                    }
                }
            },
            new CancellationToken(false),
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public ValueTask Emit(IEnumerable<FileEvent> fileEvents)
    {
        return _eventQueue.Writer.WriteAsync(fileEvents.ToArray());
    }

    public ValueTask<IAsyncDisposable> Subscribe(Func<IEnumerable<FileEvent>, ValueTask> cb)
    {
        var node = _listeners.AddLast(cb);

#pragma warning disable CA2000
        return ValueTask.FromResult<IAsyncDisposable>(new AsyncDisposable(() =>
        {
            _listeners.Remove(node);
            return ValueTask.CompletedTask;
        }));
#pragma warning restore CA2000
    }

    public async ValueTask WaitComplete()
    {
        while (_eventQueue.Reader.Count > 0 || _eventConsumerBusy)
        {
            await Task.Delay(1);
        }
    }

    protected override void Dispose(object? context)
    {
        _eventQueue.Writer.Complete();
        _eventConsumerTask.Unwrap().WaitWithoutException();
    }
}
