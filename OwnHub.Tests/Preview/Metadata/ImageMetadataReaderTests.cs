using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MetadataExtractor.Formats.Xmp;
using NUnit.Framework;
using OwnHub.Preview.Metadata;
using OwnHub.Preview.Metadata.Readers;
using Directory = MetadataExtractor.Directory;

namespace OwnHub.Tests.Preview.Metadata
{
    [TestFixture]
    public class ImageMetadataReaderTests
    {
        [Test]
        public async Task TestImageMetadataReader()
        {
            var file = TestUtils.OpenResourceRegularFile("Sony ILCE-7M3 (A7M3).jpg");

            var reader = new ImageMetadataReader();
            var metadata = await reader.ReadMetadata(file, new MetadataEntry());

            Console.WriteLine(JsonSerializer.Serialize(metadata, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        [Test]
        public void TestImageMetadataReader2()
        {
            var file = TestUtils.OpenResourceRegularFile("Sony ILCE-7M3 (A7M3).jpg");
            using var readStream = file.Open();

            IEnumerable<Directory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(readStream);

            foreach (var directory in directories)
            {
                Console.WriteLine($"{directory.Name}:");
                foreach (var tag in directory.Tags)
                {
                    Console.WriteLine($"         {tag.Name} = {tag.Description}");
                }

                if (directory is XmpDirectory xmpDirectory)
                {
                    var xmps = xmpDirectory.GetXmpProperties();
                    foreach (var xmp in xmps)
                    {
                        Console.WriteLine($"         {xmp.Key} = {xmp.Value}");
                    }
                }
            }
        }
    }
}