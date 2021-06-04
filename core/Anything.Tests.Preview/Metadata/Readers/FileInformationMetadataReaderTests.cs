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
    public class FileInformationMetadataReaderTests
    {
        [Test]
        public async Task ReaderTest()
        {
            var fileSystem = new VirtualFileSystemService();
            fileSystem.RegisterFileSystemProvider(
                "test",
                new EmbeddedFileSystemProvider(new EmbeddedFileProvider(typeof(TextFileRendererTests).Assembly)));

            async ValueTask<MetadataReaderFileInfo> MakeFileInfo(string filename)
            {
                var url = Url.Parse("file://test/Resources/" + filename);
                return new MetadataReaderFileInfo(url, await fileSystem!.Stat(url), "text/plain");
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
