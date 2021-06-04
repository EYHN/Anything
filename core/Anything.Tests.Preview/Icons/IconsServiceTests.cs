using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview.Icons;
using Anything.Tests.Preview.Thumbnails;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Icons
{
    public class IconsServiceTests
    {
        [Test]
        public async Task FeatureTest()
        {
            var fileSystem = new VirtualFileSystemService();
            fileSystem.RegisterFileSystemProvider(
                "test",
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(ThumbnailsServiceTests).Assembly)));
            fileSystem.RegisterFileSystemProvider(
                "memory",
                new MemoryFileSystemProvider());
            await fileSystem.CreateDirectory(Url.Parse("file://memory/folder"));

            var iconsService = new IconsService(fileSystem);
            iconsService.BuildCache();
            var iconId = await iconsService.GetIconId(
                Url.Parse("file://test/Resources/Test Image.png"));
            var icon = await iconsService.GetIconImage(iconId, new IconImageOption { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, icon.Size);
            Assert.AreEqual("image/png", icon.Format);
            await TestUtils.SaveResult("Image File Icon - 256w.png", icon.GetStream());

            var folderIconId = await iconsService.GetIconId(Url.Parse("file://memory/folder"));
            icon = await iconsService.GetIconImage(
                folderIconId,
                new IconImageOption { Size = 512, ImageFormat = "image/png" });
            Assert.AreEqual(512, icon.Size);
            Assert.AreEqual("image/png", icon.Format);
            await TestUtils.SaveResult("Directory Icon - 512w.png", icon.GetStream());
        }
    }
}
