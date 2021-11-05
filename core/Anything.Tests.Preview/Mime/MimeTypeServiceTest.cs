using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Preview.Mime;
using Anything.Preview.Mime.Schema;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Mime
{
    public class MimeTypeServiceTest
    {
        [Test]
        public async Task FeatureTest()
        {
            using var fileService = new FileService(TestUtils.Logger);
            fileService.AddFileSystem(
                "test",
                new EmbeddedFileSystem(new EmbeddedFileProvider(typeof(MimeTypeServiceTest).Assembly)));
            var service = new MimeTypeService(
                fileService,
                MimeTypeRules.TestRules);

            var testImage = await fileService.CreateFileHandle(Url.Parse("file://test/Resources/Test Image.png"));
            Assert.AreEqual(
                MimeType.image_png,
                await service.GetMimeType(testImage, new MimeTypeOption()));
        }
    }
}
