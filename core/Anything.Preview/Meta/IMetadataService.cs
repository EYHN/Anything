using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Meta.Schema;
using Anything.Utils;

namespace Anything.Preview.Meta
{
    public interface IMetadataService
    {
        public ValueTask<Metadata> ReadMetadata(FileHandle fileHandle);
    }
}
