using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace OwnHub.Utils
{
    public class ObjectPool<TItem>
    {
        private readonly BufferBlock<TItem> _bufferBlock;
        private readonly int _maxSize;
        private readonly Func<TItem> _creator;
        private readonly object _lock;
        private int _currentSize;

        public ObjectPool(int maxSize, Func<TItem> creator)
        {
            _lock = new object();
            _maxSize = maxSize;
            _currentSize = 1;
            _creator = creator;
            _bufferBlock = new BufferBlock<TItem>();
        }

        public void Push(TItem item)
        {
            if (!_bufferBlock.Post(item) || _bufferBlock.Count > _maxSize)
            {
                throw new Exception();
            }
        }

        public Task<TItem> PopAsync()
        {
            TItem item;
            if (_bufferBlock.TryReceive(out item))
            {
                return Task.FromResult(item);
            }
            if (_currentSize <= _maxSize)
            {
                lock (_lock)
                {
                    if (_currentSize <= _maxSize)
                    {
                        _currentSize++;
                        _bufferBlock.Post(_creator());
                    }
                }
            }

            return _bufferBlock.ReceiveAsync();
        }

        public async Task<Disposable> GetDisposableAsync()
        {
            return new Disposable(this, await PopAsync());
        }

        public class Disposable : IDisposable
        {
            private readonly ObjectPool<TItem> _pool;
            public TItem Item { get; set; }

            public Disposable(ObjectPool<TItem> pool, TItem item)
            {
                Item = item;
                _pool = pool;
            }
            public void Dispose()
            {
                _pool.Push(Item);
            }
        }
    }
}
