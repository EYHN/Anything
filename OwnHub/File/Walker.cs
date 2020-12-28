
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public class Walker: IAsyncEnumerable<IFile>
    {
        private readonly IDirectory root;
        
        public Walker(IDirectory root)
        {
            this.root = root;
        }
        
        public IAsyncEnumerator<IFile> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new WalkerEnum(root);
        }
    }
    public class WalkerEnum: IAsyncEnumerator<IFile>
    {
        private LinkedList<IFile> paths = new LinkedList<IFile>();
        private IDirectory root;
        
        public WalkerEnum(IDirectory root)
        {
            this.root = root;
            Reset();
        }
        
        private async ValueTask<IFile?> Next()
        {
            if (paths.Count == 0)
            {
                return null;
            }
            IFile item = paths.First!.Value;
            paths.RemoveFirst();
            if (item is IDirectory directory)
            {
                foreach (var entry in await directory.Entries)
                {
                    paths.AddLast(entry);
                }

                return item;
            }
            else
            {
                return item;
            }
        }

        public void Reset()
        {
            paths = new LinkedList<IFile>();
            paths.AddFirst(root);
        }

        public IFile Current { get
        {
            if (currentFile != null)
            {
                return currentFile;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }}
        
        private IFile? currentFile = null!;
        
        public async ValueTask<bool> MoveNextAsync()
        {
            currentFile = await Next();
            if (currentFile == root)
            {
                currentFile = await Next();
            }
            return currentFile != null;
        }

        public ValueTask DisposeAsync()
        {
            root = null!;
            paths = null!;
            return default;
        }
    }
}