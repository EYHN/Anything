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
            var fileSystem = new VirtualFileSystemService();
            fileSystem.RegisterFileSystemProvider(
                "test",
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));

            async ValueTask<MetadataReaderFileInfo> MakeFileInfo(string filename, string mimeType = "image/png")
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new MetadataReaderFileInfo(url, await fileSystem!.Stat(url), mimeType);
            }

            IMetadataReader reader = new ImageMetadataReader(fileSystem);
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Anything.Preview.Metadata.Schema.Metadata(),
                    await MakeFileInfo("Test Image.png"),
                    new MetadataReaderOption()))
                .ToString(true));
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Anything.Preview.Metadata.Schema.Metadata(),
                    await MakeFileInfo("Sony ILCE-7M3 (A7M3).jpg", "image/jpeg"),
                    new MetadataReaderOption()))
                .ToString(true));
            Console.WriteLine(
                (await reader.ReadMetadata(
                    new Anything.Preview.Metadata.Schema.Metadata(),
                    await MakeFileInfo("Test WebP.webp", "image/webp"),
                    new MetadataReaderOption()))
                .ToString(true));
        }
    }
}
