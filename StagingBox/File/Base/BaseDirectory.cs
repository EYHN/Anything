using System.Collections.Generic;
using System.Threading.Tasks;

namespace StagingBox.File.Base
{
    public abstract class BaseDirectory : BaseFile, IDirectory
    {
        public override MimeType? MimeType => null;
        public abstract Task<IEnumerable<IFile>> Entries { get; }
    }
}
