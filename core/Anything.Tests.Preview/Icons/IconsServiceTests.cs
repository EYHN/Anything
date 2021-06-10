using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Icons;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Preview.Icons
{
    public class IconsServiceTests
    {
        [Test]
        public async Task FeatureTest()
        {
            var fileSystem = await FileServiceFactory.BuildMemoryFileService();
            await fileSystem.FileSystem.CreateDirectory(Url.Parse("file://memory/folder"));

            await fileSystem.FileSystem.CreateDirectory(Url.Parse("file://memory/test"));
            await fileSystem.FileSystem.WriteFile(Url.Parse("file://memory/test/file"), Convert.FromHexString("010203"));

            var iconsService = new IconsService(fileSystem);
            iconsService.BuildCache();
            var iconId = await iconsService.GetIconId(
                Url.Parse("file://memory/test/file"));
            var icon = await iconsService.GetIconImage(iconId, new IconImageOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, icon.Size);
            Assert.AreEqual("image/png", icon.Format);
            await TestUtils.SaveResult("File Icon - 256w.png", icon.GetStream());

            var folderIconId = await iconsService.GetIconId(Url.Parse("file://memory/test"));
            icon = await iconsService.GetIconImage(
                folderIconId,
                new IconImageOption { Size = 512, ImageFormat = "image/png" });
            Assert.AreEqual(512, icon.Size);
            Assert.AreEqual("image/png", icon.Format);
            await TestUtils.SaveResult("Directory Icon - 512w.png", icon.GetStream());
        }
    }
}
