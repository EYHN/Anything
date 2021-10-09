using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anything.Utils.Event
{
    public class BatchEventWorkerEmitter<TArgs> : EventWorkerEmitter<TArgs[]>
    {
        public BatchEventWorkerEmitter(int queueSize)
            : base(queueSize)
        {
        }

        public ValueTask EmitAsync(TArgs args)
        {
            return EmitAsync(new[] { args });
        }

        public override IEnumerable<TArgs[]> EventsReducer(IEnumerable<TArgs[]> events)
        {
            var merged = events.SelectMany(i => i).ToArray();
            return new[] { merged };
        }
    }
}
