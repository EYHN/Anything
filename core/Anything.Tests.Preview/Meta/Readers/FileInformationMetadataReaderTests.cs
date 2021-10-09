using System;
using System.Text.Json;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Preview.Meta.Readers;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime.Schema;
using Anything.Tests.Preview.Thumbnails.Renderers;
using Anything.Utils;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Anything.Tests.Preview.Meta.Readers
{
    public class FileInformationMetadataReaderTests
    {
        [Test]
        public async Task ReaderTest()
        {
            Console.WriteLine(JsonSerializer.Serialize(Metadata.ToMetadataNamesList()));

            using var fileService = new FileService();
            fileService.AddFileSystem(
                "test",
                new EmbeddedFileSystem(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));

            async ValueTask<MetadataReaderFileInfo> MakeFileInfo(IFileService fs, string filename)
            {
                var fileHandle = await fs.CreateFileHandle(Url.Parse("file://test/Resources/" + filename));
                return new MetadataReaderFileInfo(fileHandle, await fs.Stat(fileHandle), MimeType.text_plain);
            }

            IMetadataReader reader = new FileInformationMetadataReader();
            var metadata = await reader.ReadMetadata(
                new Metadata(),
                await MakeFileInfo(fileService, "Test Text.txt"),
                new MetadataReaderOption());
            Console.WriteLine(metadata.ToString(true));
        }
    }
}
