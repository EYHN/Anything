using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace OwnHub.Utils
{
    public class ObjectPool<TItem> : IDisposable where TItem : class
    {
        private readonly Func<ValueTask<TItem>>? asynccreator;
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private readonly CancellationToken cancellationToken;
        private readonly Channel<TItem> channel;
        private readonly Func<TItem>? creator;
        private readonly int maxSize;
        private readonly AsyncLock mutex;
        private int currentSize;

        public ObjectPool(int maxSize, Func<TItem> creator): this(maxSize)
        {
            this.creator = creator;
        }

        public ObjectPool(int maxSize, Func<ValueTask<TItem>> asynccreator): this(maxSize)
        {
            this.asynccreator = asynccreator;
        }
        
        public ObjectPool(int maxSize)
        {
            mutex = new AsyncLock();
            this.maxSize = maxSize;
            currentSize = 0;
            channel = Channel.CreateBounded<TItem>(maxSize);
            cancellationToken = cancellationSource.Token;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        public void Return(TItem item)
        {
            Push(item);
        }
        
        public void Push(TItem item)
        {
            if (!Disposed)
                channel.Writer.TryWrite(item);
        }

        private async ValueTask<TItem> RawCreate()
        {
            TItem item;
            if (creator != null) item = creator();
            else if (asynccreator != null) item = await asynccreator();
            else throw new Exception("No creator available for the object pool.");
            return item;
        }

        public async ValueTask<TItem> GetAsync()
        {
            return (await GetAsync(true))!;
        }
        
        public async ValueTask<TItem?> GetAsync(bool blocking)
        {
            TItem item;
            if (channel.Reader.TryRead(out item)) return item;
            if (currentSize < maxSize)
            {
                using (await mutex.LockAsync(cancellationToken))
                {
                    if (currentSize < maxSize)
                    {
                        item = await RawCreate();
                        currentSize++;
                        return item;
                    }
                }
            }

            if (blocking == false) return null;
            return await channel.Reader.ReadAsync(cancellationToken);
        }

        public TItem Get()
        {
            return Get(true)!;
        }

        public TItem? Get(bool blocking)
        {
            TItem item;
            if (channel.Reader.TryRead(out item)) return item;
            if (currentSize < maxSize &&
                (creator != null || asynccreator != null))
            {
                using (mutex.Lock(cancellationToken))
                {
                    if (currentSize < maxSize)
                    {
                        ValueTask<TItem> task = RawCreate();
                        if (!task.IsCompleted)
                        {
                            task.AsTask().Wait(cancellationToken);
                        }

                        currentSize++;
                        return item;
                    }
                }
            }

            if (blocking == false) return null;
            Task<TItem> blockingTask = channel.Reader.ReadAsync(cancellationToken).AsTask();
            blockingTask.Wait(cancellationToken);
            return blockingTask.Result;
        }
        
        public Ref GetRef()
        {
            return GetRef(true)!;
        }
        
        public Ref? GetRef(bool blocking)
        {
            TItem? item = Get(blocking);
            return item != null ? new Ref(this, item) : null;
        }

        public async ValueTask<Ref> GetRefAsync()
        {
            return (await GetRefAsync(true))!;
        }
        
        public async ValueTask<Ref?> GetRefAsync(bool blocking)
        {
            TItem? item = await GetAsync(blocking);
            return item != null ? new Ref(this, item) : null;
        }

        ~ObjectPool()
        {
            DoDispose();
        }

        protected virtual void DoDispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                while (channel.Reader.TryRead(out var _)) ;
                cancellationSource.Cancel();
            }
        }

        public class Ref : IDisposable
        {
            private readonly ObjectPool<TItem> parent;

            public Ref(ObjectPool<TItem> pool, TItem item)
            {
                _value = item;
                parent = pool;
                Returned = false;
            }

            private readonly TItem _value;
            public TItem Value {
                get
                {
                    if (Returned) throw new InvalidOperationException("Container has expired.");
                    return _value;
                }
            }

            private bool Returned { get; set; }

            public void Dispose()
            {
                Return();
                GC.SuppressFinalize(this);
            }

            ~Ref()
            {
                Return();
            }

            public void Return()
            {
                if (Returned) return;
                
                parent.Return(_value);
                Returned = true;
            }
        }
    }
}