using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Preview.Icons
{
    public static class IconsServiceFactory
    {
        public static ValueTask<IIconsService> BuildIconsService(IFileSystemService fileSystem)
        {
            var iconsService = new IconsService(fileSystem);
            iconsService.BuildCache();
            return ValueTask.FromResult(iconsService as IIconsService);
        }
    }
}
