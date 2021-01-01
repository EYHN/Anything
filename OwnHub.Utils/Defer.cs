using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace OwnHub.Utils
{
    public class Defer
    {
        private readonly IDisposable disposable;
        private readonly Task task;

        public bool IsCompleted => task.IsCompleted;
        
        public Defer()
        {
            AsyncLock mutex = new AsyncLock();
            disposable = mutex.Lock();
            task = mutex.LockAsync();
        }

        public void Resolve()
        {
            disposable.Dispose();
        }

        public Task Wait(CancellationToken cancellationToken = default)
        {
            return task.ContinueWith((_) => Task.CompletedTask, cancellationToken);
        }
    }
}