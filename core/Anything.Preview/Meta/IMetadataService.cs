using System.Threading.Tasks;
using Anything.Preview.Meta.Schema;
using Anything.Utils;

namespace Anything.Preview.Meta
{
    public interface IMetadataService
    {
        public ValueTask<Metadata> ReadMetadata(Url url);
    }
}
