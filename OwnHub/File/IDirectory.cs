using System.Collections.Generic;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public interface IDirectory : IFile
    {
        public Task<IEnumerable<IFile>> Entries { get; }
    }
}