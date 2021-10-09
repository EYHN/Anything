using System;
using System.Threading.Tasks;

namespace Anything.Utils.Event
{
    public class EventEmitter<TArgs>
    {
        public Event<TArgs> Event { get; } = new();

        public void Emit(TArgs args)
        {
            Event.Emit(args);
        }

        public Task EmitAsync(TArgs args)
        {
            return Event.EmitAsync(args);
        }

        public EventDisposable Extends(Event<TArgs> @event, Func<TArgs, TArgs>? mapper = null)
        {
            if (mapper == null)
            {
                return @event.On(EmitAsync);
            }
            else
            {
                return @event.On(args => EmitAsync(mapper(args)));
            }
        }
    }
}
