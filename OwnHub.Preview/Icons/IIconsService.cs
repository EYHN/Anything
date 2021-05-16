using OwnHub.Utils;

namespace OwnHub.Preview.Icons
{
    public interface IIconsService
    {
        public string GetIcons(Url url, IconsOption option);
    }
}
