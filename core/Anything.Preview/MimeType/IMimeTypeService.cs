using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.Preview.MimeType
{
    public interface IMimeTypeService
    {
        public ValueTask<MimeType.Schema.MimeType?> GetMimeType(Url url, MimeTypeOption option);
    }
}
