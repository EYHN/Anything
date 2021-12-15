using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.Preview;
using Anything.Preview.Icons;
using Anything.Utils;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Anything.Tests.Preview.Icons;

public class IconsServiceTests
{
    [Test]
    public async Task FeatureTest()
    {
        var services = new ServiceCollection();
        services.TryAddSingletonFileService(builder => builder.TryAddMemoryFileSystem("memory"));
        services.TryAddIconsFeature();
        services.AddTestLogging();
        await using var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        var iconsService = serviceProvider.GetRequiredService<IIconsService>();

        var root = await fileService.CreateFileHandle(Url.Parse("file://memory/"));
        var testFolder = await fileService.CreateDirectory(root, "folder");

        var testFile = await fileService.CreateFile(testFolder, "file", Convert.FromHexString("010203"));

        var iconId = await iconsService.GetIconId(testFile);
        var icon = await iconsService.GetIconImage(iconId, new IconImageOption { Size = 256, ImageFormat = "image/png" });
        Assert.AreEqual(256, icon.Size);
        Assert.AreEqual("image/png", icon.ImageFormat);
        await TestUtils.SaveResult("File Icon - 256w.png", icon.Data);

        var folderIconId = await iconsService.GetIconId(testFolder);
        icon = await iconsService.GetIconImage(
            folderIconId,
            new IconImageOption { Size = 512, ImageFormat = "image/png" });
        Assert.AreEqual(512, icon.Size);
        Assert.AreEqual("image/png", icon.ImageFormat);
        await TestUtils.SaveResult("Directory Icon - 512w.png", icon.Data);
    }
}
