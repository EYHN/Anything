using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.Preview.Metadata
{
    public interface IMetadataService
    {
        public ValueTask<Schema.Metadata> ReadMetadata(Url url);
    }
}
