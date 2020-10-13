using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File.Base
{
    public abstract class BaseDirectory : BaseFile, IDirectory
    {
        public override MimeType? MimeType => null;
        public abstract Task<IEnumerable<IFile>> Entries { get; }
    }
}
