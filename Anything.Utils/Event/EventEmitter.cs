using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
