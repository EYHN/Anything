using System;

namespace Anything.Utils.Event
{
    public class EventDisposable : Disposable
    {
        public EventDisposable(Action? callOnDispose = null)
            : base(callOnDispose)
        {
        }
    }
}
