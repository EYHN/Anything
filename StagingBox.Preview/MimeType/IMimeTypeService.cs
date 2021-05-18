using System.Threading.Tasks;
using StagingBox.Utils;

namespace StagingBox.Preview.MimeType
{
    public interface IMimeTypeService
    {
        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option);
    }
}
