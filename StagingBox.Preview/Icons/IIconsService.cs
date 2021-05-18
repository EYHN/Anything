using System.Threading.Tasks;
using StagingBox.Utils;

namespace StagingBox.Preview.Icons
{
    public interface IIconsService
    {
        public ValueTask<IIcon> GetIcon(Url url, IconsOption option);
    }
}
