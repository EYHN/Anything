using System.Threading.Tasks;
using OwnHub.Utils;

namespace OwnHub.Preview.MimeType
{
    public interface IMimeTypeService
    {
        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option);
    }
}
