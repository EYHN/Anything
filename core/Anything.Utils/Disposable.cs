using System;

namespace Anything.Utils
{
    public class Disposable : IDisposable
    {
        private readonly Action _callOnDispose;
        private bool _disposed;

        public Disposable(Action callOnDispose)
        {
            _callOnDispose = callOnDispose;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _callOnDispose();
                }

                _disposed = true;
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
