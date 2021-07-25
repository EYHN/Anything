using System.Collections.Generic;
using System.Threading;
using Anything.Utils;

namespace Anything.FileSystem.Walker
{
    public static class FileSystemWalkerFactory
    {
        public static IFileSystemWalker FromEnumerable(Url rootUrl, IAsyncEnumerable<Url> enumerable)
        {
            return new FileSystemWalkerFromEnumerable(rootUrl, enumerable);
        }

        private class FileSystemWalkerFromEnumerable : IFileSystemWalker
        {
            private readonly IAsyncEnumerable<Url> _enumerable;

            public FileSystemWalkerFromEnumerable(Url rootUrl, IAsyncEnumerable<Url> enumerable)
            {
                _enumerable = enumerable;
                RootUrl = rootUrl;
            }

            /// <inheritdoc />
            public Url RootUrl { get; }

            /// <inheritdoc />
            public IAsyncEnumerator<Url> GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                return _enumerable.GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}
