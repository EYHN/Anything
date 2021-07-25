using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anything.Utils.Event
{
    public class Event<TArgs>
    {
        private readonly List<Func<TArgs, Task>> _asyncHandlers = new();
        private readonly List<Action<TArgs>> _handlers = new();

        public IDisposable On(Func<TArgs, Task> handler)
        {
            _asyncHandlers.Add(handler);

            return new Disposable(() =>
            {
                _asyncHandlers.Remove(handler);
            });
        }

        public IDisposable On(Action<TArgs> handler)
        {
            _handlers.Add(handler);

            return new Disposable(() =>
            {
                _handlers.Remove(handler);
            });
        }

        internal void Emit(TArgs args)
        {
            foreach (var handler in _handlers)
            {
                handler.Invoke(args);
            }

            Task.WhenAll(_asyncHandlers.Select(handler => handler.Invoke(args))).Wait();
        }

        internal Task EmitAsync(TArgs args)
        {
            foreach (var handler in _handlers)
            {
                handler.Invoke(args);
            }

            return Task.WhenAll(_asyncHandlers.Select(handler => handler.Invoke(args)));
        }
    }
}
