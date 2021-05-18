using System.Threading.Tasks;
using OwnHub.Utils;

namespace OwnHub.Preview.Icons
{
    public interface IIconsService
    {
        public ValueTask<IIcon> GetIcons(Url url, IconsOption option);
    }
}
