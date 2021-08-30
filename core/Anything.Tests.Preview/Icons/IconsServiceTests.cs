using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
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
            using var fileService = new FileService();
            fileService.AddTestFileSystem(
                Url.Parse("file://memory/"),
                new MemoryFileSystemProvider());

            await fileService.CreateDirectory(Url.Parse("file://memory/folder"));

            await fileService.CreateDirectory(Url.Parse("file://memory/test"));
            await fileService.WriteFile(Url.Parse("file://memory/test/file"), Convert.FromHexString("010203"));

            var iconsService = new IconsService(fileService);
            var iconId = await iconsService.GetIconId(
                Url.Parse("file://memory/test/file"));
            var icon = await iconsService.GetIconImage(iconId, new IconImageOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, icon.Size);
            Assert.AreEqual("image/png", icon.Format);
            await using (var stream = icon.GetStream())
            {
                await TestUtils.SaveResult("File Icon - 256w.png", stream);
            }

            var folderIconId = await iconsService.GetIconId(Url.Parse("file://memory/test"));
            icon = await iconsService.GetIconImage(
                folderIconId,
                new IconImageOption { Size = 512, ImageFormat = "image/png" });
            Assert.AreEqual(512, icon.Size);
            Assert.AreEqual("image/png", icon.Format);
            await using (var stream = icon.GetStream())
            {
                await TestUtils.SaveResult("Directory Icon - 512w.png", stream);
            }
        }
    }
}
