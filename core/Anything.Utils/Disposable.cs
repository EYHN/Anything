using System;

namespace Anything.Utils
{
    public class Disposable : IDisposable
    {
        private readonly Action? _callOnDispose;

        public Disposable(Action? callOnDispose = null)
        {
            _callOnDispose = callOnDispose;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManaged()
        {
            _callOnDispose?.Invoke();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    DisposeManaged();
                }

                Disposed = true;
            }
        }

        ~Disposable()
        {
            Dispose(false);
        }

        public static Disposable From(params IDisposable[] disposables)
        {
            return new(() =>
            {
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
            });
        }
    }
}
