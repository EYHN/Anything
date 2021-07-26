using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview.Metadata.Readers;
using Anything.Tests.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Metadata.Readers
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

            async ValueTask<MetadataReaderFileInfo> MakeFileInfo(IFileService fs, string filename, string mimeType = "image/png")
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new MetadataReaderFileInfo(url, await fs.Stat(url), mimeType);
            }

            IMetadataReader reader = new ImageMetadataReader(fileService);
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Anything.Preview.Metadata.Schema.Metadata(),
                    await MakeFileInfo(fileService, "Test Image.png"),
                    new MetadataReaderOption()))
                .ToString(true));
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Anything.Preview.Metadata.Schema.Metadata(),
                    await MakeFileInfo(fileService, "Sony ILCE-7M3 (A7M3).jpg", "image/jpeg"),
                    new MetadataReaderOption()))
                .ToString(true));
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Anything.Preview.Metadata.Schema.Metadata(),
                    await MakeFileInfo(fileService, "Test WebP.webp", "image/webp"),
                    new MetadataReaderOption()))
                .ToString(true));
        }
    }
}
