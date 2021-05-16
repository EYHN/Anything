using OwnHub.Utils;

namespace OwnHub.Preview.MimeType
{
    public interface IMimeTypeService
    {
        public string GetMimeType(Url url, MimeTypeOption option);
    }
}
