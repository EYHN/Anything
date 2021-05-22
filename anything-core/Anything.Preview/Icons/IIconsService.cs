using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.Preview.Icons
{
    public interface IIconsService
    {
        public ValueTask<IIcon> GetIcon(Url url, IconsOption option);
    }
}
