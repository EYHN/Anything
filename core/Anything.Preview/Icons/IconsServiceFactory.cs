using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Preview.Icons
{
    public static class IconsServiceFactory
    {
        public static ValueTask<IIconsService> BuildIconsService(IFileService fileService)
        {
            var iconsService = new IconsService(fileService);
            iconsService.BuildCache();
            return ValueTask.FromResult(iconsService as IIconsService);
        }
    }
}
