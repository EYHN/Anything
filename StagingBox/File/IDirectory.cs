using System.Collections.Generic;
using System.Threading.Tasks;

namespace StagingBox.File
{
    public interface IDirectory : IFile
    {
        public Task<IEnumerable<IFile>> Entries { get; }
    }
}
