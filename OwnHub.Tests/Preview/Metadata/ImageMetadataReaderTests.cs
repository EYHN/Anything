using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MetadataExtractor.Formats.Xmp;
using NUnit.Framework;
using OwnHub.File;
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
            IRegularFile? file = TestUtils.ReadResourceRegularFile("Sony ILCE-7M3 (A7M3).jpg");

            var reader = new ImageMetadataReader();
            MetadataEntry? metadata = await reader.ReadMetadata(file, new MetadataEntry());

            Console.WriteLine(JsonSerializer.Serialize(metadata, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        [Test]
        public void TestImageMetadataReader2()
        {
            IRegularFile? file = TestUtils.ReadResourceRegularFile("Sony ILCE-7M3 (A7M3).jpg");
            using Stream? readStream = file.Open();

            IEnumerable<Directory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(readStream);

            foreach (var directory in directories)
            {
                Console.WriteLine($"{directory.Name}:");
                foreach (var tag in directory.Tags)
                    Console.WriteLine($"         {tag.Name} = {tag.Description}");

                if (directory is XmpDirectory xmpDirectory)
                {
                    IDictionary<string, string>? xmps = xmpDirectory.GetXmpProperties();
                    foreach (KeyValuePair<string, string> xmp in xmps)
                        Console.WriteLine($"         {xmp.Key} = {xmp.Value}");
                }
            }
        }
    }
}