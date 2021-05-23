using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Anything.Utils.Async
{
    public class Defer
    {
        private readonly IDisposable _disposable;
        private readonly Task _task;

        public Defer()
        {
            AsyncLock mutex = new();
            _disposable = mutex.Lock();
            _task = mutex.LockAsync();
        }

        public bool IsCompleted => _task.IsCompleted;

        public void Resolve()
        {
            _disposable.Dispose();
        }

        public Task Wait(CancellationToken cancellationToken = default)
        {
            return _task.ContinueWith(_ => Task.CompletedTask, cancellationToken);
        }
    }
}
