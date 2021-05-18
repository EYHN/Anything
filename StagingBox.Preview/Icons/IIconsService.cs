using System.Threading.Tasks;
using StagingBox.Utils;

namespace StagingBox.Preview.Icons
{
    public interface IIconsService
    {
        public ValueTask<IIcon> GetIcons(Url url, IconsOption option);
    }
}
