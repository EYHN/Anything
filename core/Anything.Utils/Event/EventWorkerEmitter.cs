using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Anything.Utils.Event
{
    public class EventWorkerEmitter<TArgs> : Disposable
    {
        private readonly Channel<TArgs> _eventQueue;
        private readonly EventEmitter<TArgs> _emitter;
        private bool _eventConsumerBusy;
        private readonly Task<Task> _eventConsumerTask;

        public Event<TArgs> Event => _emitter.Event;

        public EventWorkerEmitter(int queueSize = 100)
        {
            _eventQueue = Channel.CreateBounded<TArgs>(queueSize);
            _emitter = new EventEmitter<TArgs>();

            _eventConsumerTask = Task.Factory.StartNew(
                async () =>
                {
                    while (await _eventQueue.Reader.WaitToReadAsync())
                    {
                        _eventConsumerBusy = true;
                        try
                        {
                            List<TArgs> events = new();
                            while (_eventQueue.Reader.TryRead(out var nextEvent))
                            {
                                events.Add(nextEvent);
                            }

                            if (events.Count == 0)
                            {
                                continue;
                            }

                            var finalEvents = EventsReducer(events);

                            foreach (var @event in finalEvents)
                            {
                                await _emitter.EmitAsync(@event);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine(ex);
                        }

                        _eventConsumerBusy = false;
                    }
                },
                new CancellationToken(false),
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public async ValueTask WaitComplete()
        {
            while (_eventQueue.Reader.Count > 0 ||
                   _eventConsumerBusy)
            {
                await Task.Delay(1);
            }
        }

        public ValueTask EmitAsync(TArgs args)
        {
            return _eventQueue.Writer.WriteAsync(args);
        }

        public virtual IEnumerable<TArgs> EventsReducer(IEnumerable<TArgs> events)
        {
            return events;
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _eventQueue.Writer.Complete();
            _eventConsumerTask.Unwrap().Wait();
        }
    }
}
