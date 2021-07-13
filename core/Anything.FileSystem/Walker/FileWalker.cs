using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.FileSystem.Walker
{
    public class FileWalker : IAsyncEnumerable<FileWalker.WalkerItem>
    {
        private readonly IFileSystem _fileSystem;
        private readonly Url _rootUrl;

        public FileWalker(IFileSystem fileSystem, Url rootUrl)
        {
            _fileSystem = fileSystem;
            _rootUrl = rootUrl;
        }

        public IAsyncEnumerator<WalkerItem> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new WalkerEnum(_fileSystem, _rootUrl);
        }

        public record WalkerItem(Url Url, (string Name, FileStats Stats)[] Entries);

        public class WalkerEnum : IAsyncEnumerator<WalkerItem>
        {
            private readonly IFileSystem _fileSystem;

            private WalkerItem? _currentFile;
            private readonly Url _rootUrl;
            private LinkedList<Url> _stacks = new();

            public WalkerEnum(IFileSystem fileSystem, Url rootUrl)
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

                Url url = _stacks.First!.Value;
                _stacks.RemoveFirst();
                var children = (await _fileSystem.ReadDirectory(url)).ToArray();
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
