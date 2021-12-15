using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Preview.Icons;

public interface IIconsService
{
    public ValueTask<string> GetIconId(FileHandle fileHandle);

    public ValueTask<IconImage> GetIconImage(string id, IconImageOption option);
}
