using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public interface IDirectory: IFile
    {
        public Task<IEnumerable<IFile>> Entries { get; }
    }
}
