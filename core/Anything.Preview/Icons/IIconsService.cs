using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.Preview.Icons
{
    public interface IIconsService
    {
        public ValueTask<string> GetIconId(Url url);

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option);
    }
}
