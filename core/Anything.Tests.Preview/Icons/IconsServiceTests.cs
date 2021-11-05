using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
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
            using var fileService = new FileService(TestUtils.Logger);
            fileService.AddFileSystem(
                "memory",
                new MemoryFileSystem());

            var root = await fileService.CreateFileHandle(Url.Parse("file://memory/"));
            var testFolder = await fileService.CreateDirectory(root, "folder");

            var testFile = await fileService.CreateFile(testFolder, "file", Convert.FromHexString("010203"));

            var iconsService = new IconsService(fileService);
            var iconId = await iconsService.GetIconId(testFile);
            var icon = await iconsService.GetIconImage(iconId, new IconImageOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, icon.Size);
            Assert.AreEqual("image/png", icon.Format);
            await using (var stream = icon.GetStream())
            {
                await TestUtils.SaveResult("File Icon - 256w.png", stream);
            }

            var folderIconId = await iconsService.GetIconId(testFolder);
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
