using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview.Meta.Readers;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime.Schema;
using Anything.Tests.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Meta.Readers
{
    public class ImageMetadataReaderTests
    {
        [Test]
        public async Task ReaderTest()
        {
            using var fileService = new FileService();
            fileService.AddTestFileSystem(
                Url.Parse("file://test/"),
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));

            async ValueTask<MetadataReaderFileInfo> MakeFileInfo(
                IFileService fs,
                string filename,
                MimeType? mimeType = null)
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new MetadataReaderFileInfo(url, await fs.Stat(url), mimeType ?? MimeType.image_png);
            }

            IMetadataReader reader = new ImageMetadataReader(fileService);
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Metadata(),
                    await MakeFileInfo(fileService, "Test Image.png"),
                    new MetadataReaderOption()))
                .ToString(true));
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Metadata(),
                    await MakeFileInfo(fileService, "Sony ILCE-7M3 (A7M3).jpg", MimeType.image_jpeg),
                    new MetadataReaderOption()))
                .ToString(true));
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Metadata(),
                    await MakeFileInfo(fileService, "Test WebP.webp", MimeType.image_webp),
                    new MetadataReaderOption()))
                .ToString(true));
        }
    }
}
