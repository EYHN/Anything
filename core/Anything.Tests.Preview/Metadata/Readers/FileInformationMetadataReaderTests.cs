using System;
using System.Text.Json;
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
    public class FileInformationMetadataReaderTests
    {
        [Test]
        public async Task ReaderTest()
        {
            Console.WriteLine(JsonSerializer.Serialize(Anything.Preview.Metadata.Schema.Metadata.ToMetadataNamesList()));

            using var fileService = new FileService();
            fileService.AddTestFileSystem(
                Url.Parse("file://test/"),
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));

            async ValueTask<MetadataReaderFileInfo> MakeFileInfo(string filename)
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new MetadataReaderFileInfo(url, await fileService.Stat(url), "text/plain");
            }

            IMetadataReader reader = new FileInformationMetadataReader();
            var metadata = await reader.ReadMetadata(
                new Anything.Preview.Metadata.Schema.Metadata(),
                await MakeFileInfo("Test Text.txt"),
                new MetadataReaderOption());
            Console.WriteLine(metadata.ToString(true));
        }
    }
}
