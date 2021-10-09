using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;

namespace Anything.Preview.Icons
{
    public interface IIconsService
    {
        public ValueTask<string> GetIconId(FileHandle fileHandle);

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option);
    }
}
