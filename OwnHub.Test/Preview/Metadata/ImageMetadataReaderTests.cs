using OwnHub.File;
using OwnHub.File.Virtual;
using OwnHub.Preview.Metadata;
using MetadataExtractor.Formats.Xmp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using OwnHub.Tests;
using OwnHub.Test.Preview.Metadata;

namespace OwnHub.Preview.Metadata.Tests
{
    [TestClass]
    public class ImageMetadataReaderTests
    {
        [TestMethod]
        public void TestImageMetadataReader()
        {
            IRegularFile file = TestUtils.ReadResourceRegularFile("Sony ILCE-7M3 (A7M3).jpg");

            ImageMetadataReader Reader = new ImageMetadataReader();
            var Metadata = Reader.ReadImageMetadata(file, new MetadataEntry());

            Console.WriteLine(JsonSerializer.Serialize(Metadata, new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        [TestMethod]
        public void TestImageMetadataReader2()
        {
            IRegularFile File = TestUtils.ReadResourceRegularFile("Sony ILCE-7M3 (A7M3).jpg");
            using (Stream ReadStream = File.Open())
            {
                IEnumerable<MetadataExtractor.Directory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(ReadStream);

                foreach (var directory in directories)
                {
                    Console.WriteLine($"{directory.Name}:");
                    foreach (var tag in directory.Tags)
                        Console.WriteLine($"         {tag.Name} = {tag.Description}");

                    if (directory is XmpDirectory)
                    {
                        var xmps = (directory as XmpDirectory).GetXmpProperties();
                        foreach (var xmp in xmps)
                        {
                            Console.WriteLine($"         {xmp.Key} = {xmp.Value}");
                        }
                    }
                }
            }
            
        }
    }
}
