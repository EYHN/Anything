using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace OwnHub.Utils
{
    public class ObjectPool<TItem> : IDisposable where TItem : class
    {
        private readonly Func<Task<TItem>>? asynccreator;
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

        public ObjectPool(int maxSize, Func<Task<TItem>> asynccreator): this(maxSize)
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
                if (!channel.Writer.TryWrite(item))
                    throw new Exception();
        }

        public async Task<TItem> GetAsync()
        {
            return (await GetAsync(true))!;
        }
        
        public async Task<TItem?> GetAsync(bool blocking)
        {
            TItem item;
            if (channel.Reader.TryRead(out item)) return item;
            if (currentSize < maxSize)
            {
                using (await mutex.LockAsync(cancellationToken))
                {
                    if (currentSize < maxSize)
                    {
                        if (creator != null) item = creator();
                        else if (asynccreator != null) item = await asynccreator();
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
                        if (creator != null) item = creator();
                        else if (asynccreator != null)
                        {
                            Task<TItem> task = asynccreator();
                            task.Wait(cancellationToken);
                            item = task.Result;
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
        
        public Container GetContainer()
        {
            return GetContainer(true)!;
        }
        
        public Container? GetContainer(bool blocking)
        {
            TItem? item = Get(blocking);
            return item != null ? new Container(this, item) : null;
        }

        public async Task<Container> GetContainerAsync()
        {
            return (await GetContainerAsync(true))!;
        }
        
        public async Task<Container?> GetContainerAsync(bool blocking)
        {
            TItem? item = await GetAsync(blocking);
            return item != null ? new Container(this, item) : null;
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

        public class Container : IDisposable
        {
            private readonly ObjectPool<TItem> parent;

            public Container(ObjectPool<TItem> pool, TItem item)
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

            ~Container()
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