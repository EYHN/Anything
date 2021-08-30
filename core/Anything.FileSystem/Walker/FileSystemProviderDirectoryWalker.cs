using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Anything.FileSystem.Provider;
using Anything.Utils;

namespace Anything.FileSystem.Walker
{
    public class FileSystemProviderDirectoryWalker : IAsyncEnumerable<FileSystemProviderDirectoryWalker.WalkerItem>
    {
        private readonly IFileSystemProvider _fileSystem;
        private readonly Url _rootUrl;

        public FileSystemProviderDirectoryWalker(IFileSystemProvider fileSystem, Url rootUrl)
        {
            _fileSystem = fileSystem;
            _rootUrl = rootUrl;
        }

        public IAsyncEnumerator<WalkerItem> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new WalkerEnum(_fileSystem, _rootUrl);
        }

        public WalkerThread StartWalkerThread(Func<WalkerItem, Task> callback)
        {
            return new(this, callback);
        }

        public class WalkerThread : Disposable
        {
            private const int DefaultInterval = 100;
            private const int FastInterval = 0;

            private readonly Func<WalkerItem, Task> _callback;
            private readonly CancellationTokenSource _cancellationTokenSource = new();
            private readonly FileSystemProviderDirectoryWalker _walker;
            private int _fastScan; // 0 as open, 1 as off
            private Task? _loopTask;

            public WalkerThread(FileSystemProviderDirectoryWalker walker, Func<WalkerItem, Task> callback)
            {
                _walker = walker;
                _callback = callback;
                _loopTask = Task.Factory.StartNew(
                    Loop,
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }

            public long LoopCount { get; private set; }

            public async Task Loop()
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    // atomic operation. equal with (fast = _fastScan, _fastScan = 0)
                    var fast = Interlocked.Exchange(ref _fastScan, 0);

                    await foreach (var item in _walker)
                    {
                        var stopWatch = new Stopwatch();
                        stopWatch.Start();
                        try
                        {
                            await _callback(item);
                        }
                        catch (System.Exception exception)
                        {
                            Console.Error.WriteLine(exception);
                        }

                        stopWatch.Stop();
                        var callbackTime = stopWatch.Elapsed.TotalMilliseconds;

                        await Task.Delay(fast != 0 || _fastScan != 0
                            ? FastInterval
                            : DefaultInterval + (int)Math.Min(callbackTime, 1000.0));
                    }

                    LoopCount++;
                }
            }

            public async ValueTask WaitFullWalk()
            {
                var start = LoopCount;

                // Speed up the walk until the next full walk is completed.
                _fastScan = 1;
                while (LoopCount < start + 2)
                {
                    await Task.Delay(1);
                }
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();

                _cancellationTokenSource.Dispose();
                _loopTask?.Wait();
                _loopTask = null;
            }
        }

        public record WalkerItem(Url Url, ImmutableArray<(string Name, FileStats Stats)> Entries);

        private class WalkerEnum : IAsyncEnumerator<WalkerItem>
        {
            private readonly IFileSystemProvider _fileSystem;
            private readonly Url _rootUrl;

            private WalkerItem? _currentFile;
            private LinkedList<Url> _stacks = new();

            public WalkerEnum(IFileSystemProvider fileSystem, Url rootUrl)
            {
                _fileSystem = fileSystem;
                _rootUrl = rootUrl;
                Reset();
            }

            public WalkerItem Current
            {
                get
                {
                    if (_currentFile != null)
                    {
                        return _currentFile;
                    }

                    throw new InvalidOperationException();
                }
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                _currentFile = await Next();

                return _currentFile != null;
            }

            public ValueTask DisposeAsync()
            {
                return default;
            }

            private async ValueTask<WalkerItem?> Next()
            {
                if (_stacks.Count == 0)
                {
                    return null;
                }

                var url = _stacks.First!.Value;
                _stacks.RemoveFirst();
                var children = (await _fileSystem.ReadDirectory(url)).ToImmutableArray();
                foreach (var child in children)
                {
                    if (child.Stats.Type == FileType.Directory)
                    {
                        _stacks.AddLast(url.JoinPath(child.Name));
                    }
                }

                return new WalkerItem(url, children);
            }

            public void Reset()
            {
                _stacks = new LinkedList<Url>();
                _stacks.AddFirst(_rootUrl);
            }
        }
    }
}
