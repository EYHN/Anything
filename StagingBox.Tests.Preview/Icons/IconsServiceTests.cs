using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using StagingBox.FileSystem;
using StagingBox.FileSystem.Provider;
using StagingBox.Preview.Icons;
using StagingBox.Tests.Preview.Thumbnails;
using StagingBox.Utils;

namespace StagingBox.Tests.Preview.Icons
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
            var icon = await iconsService.GetIcons(
                Url.Parse("file://test/Resources/Test Image.png"),
                new IconsOption() { Size = 256, ImageFormat = "image/png" });
            Assert.AreEqual(256, icon.Size);
            Assert.AreEqual("image/png", icon.ImageFormat);
            await TestUtils.SaveResult("Image File Icon - 256w.png", icon.GetStream());

            icon = await iconsService.GetIcons(
                Url.Parse("file://memory/folder"),
                new IconsOption() { Size = 512, ImageFormat = "image/png" });
            Assert.AreEqual(512, icon.Size);
            Assert.AreEqual("image/png", icon.ImageFormat);
            await TestUtils.SaveResult("Directory Icon - 512w.png", icon.GetStream());
        }
    }
}
