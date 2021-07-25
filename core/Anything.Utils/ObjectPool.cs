using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Anything.Utils
{
    public class ObjectPool<TItem> : IDisposable where TItem : class
    {
        private readonly Func<ValueTask<TItem>>? _asynccreator;
        private readonly CancellationTokenSource _cancellationSource = new();
        private readonly CancellationToken _cancellationToken;
        private readonly Channel<TItem> _channel;
        private readonly Func<TItem>? _creator;
        private readonly int _maxSize;
        private readonly AsyncLock _mutex;
        private int _currentSize;

        public ObjectPool(int maxSize, Func<TItem> creator) : this(maxSize)
        {
            _creator = creator;
        }

        public ObjectPool(int maxSize, Func<ValueTask<TItem>> asynccreator) : this(maxSize)
        {
            _asynccreator = asynccreator;
        }

        public ObjectPool(int maxSize)
        {
            _mutex = new AsyncLock();
            _maxSize = maxSize;
            _currentSize = 0;
            _channel = Channel.CreateBounded<TItem>(maxSize);
            _cancellationToken = _cancellationSource.Token;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Return(TItem item)
        {
            Push(item);
        }

        public void Push(TItem item)
        {
            if (!Disposed)
            {
                _channel.Writer.TryWrite(item);
            }
        }

        private async ValueTask<TItem> RawCreate()
        {
            TItem item;
            if (_creator != null)
            {
                item = _creator();
            }
            else
            {
                item = _asynccreator != null
                    ? await _asynccreator()
                    : throw new InvalidOperationException("No creator available for the object pool.");
            }

            return item;
        }

        public async ValueTask<TItem> GetAsync()
        {
            return (await GetAsync(true))!;
        }

        public async ValueTask<TItem?> GetAsync(bool blocking)
        {
            if (_channel.Reader.TryRead(out var item))
            {
                return item;
            }

            if (_currentSize < _maxSize)
            {
                using (await _mutex.LockAsync(_cancellationToken))
                {
                    if (_currentSize < _maxSize)
                    {
                        item = await RawCreate();
                        _currentSize++;
                        return item;
                    }
                }
            }

            return blocking == false ? null : await _channel.Reader.ReadAsync(_cancellationToken);
        }

        public TItem Get()
        {
            return Get(true)!;
        }

        public TItem? Get(bool blocking)
        {
#pragma warning disable IDE0018
            TItem? item;
#pragma warning restore IDE0018
            if (_channel.Reader.TryRead(out item))
            {
                return item;
            }

            if (_currentSize < _maxSize &&
                (_creator != null || _asynccreator != null))
            {
                using (_mutex.Lock(_cancellationToken))
                {
                    if (_currentSize < _maxSize)
                    {
                        var task = RawCreate();
                        if (!task.IsCompleted)
                        {
                            task.AsTask().Wait(_cancellationToken);
                        }

                        _currentSize++;
                        return item;
                    }
                }
            }

            if (blocking == false)
            {
                return null;
            }

            var blockingTask = _channel.Reader.ReadAsync(_cancellationToken).AsTask();
            blockingTask.Wait(_cancellationToken);
            return blockingTask.Result;
        }

        public Ref GetRef()
        {
            return GetRef(true)!;
        }

        public Ref? GetRef(bool blocking)
        {
            var item = Get(blocking);
            return item != null ? new Ref(this, item) : null;
        }

        public async ValueTask<Ref> GetRefAsync()
        {
            return (await GetRefAsync(true))!;
        }

        public async ValueTask<Ref?> GetRefAsync(bool blocking)
        {
            var item = await GetAsync(blocking);
            return item != null ? new Ref(this, item) : null;
        }

        ~ObjectPool()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    while (_channel.Reader.TryRead(out var _)) { }

                    _cancellationSource.Cancel();
                    _cancellationSource.Dispose();
                }

                Disposed = true;
            }
        }

        public class Ref : IDisposable
        {
            private readonly ObjectPool<TItem> _parent;

            private readonly TItem _value;

            private bool _disposed;

            public Ref(ObjectPool<TItem> pool, TItem item)
            {
                _value = item;
                _parent = pool;
                Returned = false;
            }

            public TItem Value => Returned ? throw new InvalidOperationException("Container has expired.") : _value;

            private bool Returned { get; set; }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public event Action<TItem>? OnReturn;

            private void Return()
            {
                if (Returned)
                {
                    return;
                }

                OnReturn?.Invoke(_value);
                _parent.Return(_value);
                Returned = true;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        Return();
                    }

                    _disposed = true;
                }
            }

            ~Ref()
            {
                Dispose(false);
            }
        }
    }
}
