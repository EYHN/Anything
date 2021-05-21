using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.Preview.MimeType
{
    public interface IMimeTypeService
    {
        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option);
    }
}
