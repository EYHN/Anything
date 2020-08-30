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

namespace OwnHub.Preview.Metadata.Tests
{
    [TestClass]
    public class ImageMetadataReaderTests
    {
        //[TestMethod]
        //public void TestImageMetadataReader()
        //{
        //    IRegularFile file = TestUtils.ReadResourceRegularFile("grayscale with alpha.png");

        //    ImageMetadataReader reader = new ImageMetadataReader();

        //    var metadata = reader.ReadImageMetadata(file);

        //    Console.WriteLine(JsonSerializer.Serialize(metadata, new JsonSerializerOptions() {
        //        WriteIndented = true
        //    }));
        //}

        //[TestMethod]
        //public void TestImageMetadataReader2()
        //{
        //    IRegularFile file = TestUtils.ReadResourceRegularFile("grayscale with alpha.png");

        //    IEnumerable<MetadataExtractor.Directory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(file.Open());

        //    foreach (var directory in directories)
        //    {
        //        Console.WriteLine($"{directory.Name}:");
        //        foreach (var tag in directory.Tags)
        //            Console.WriteLine($"         {tag.Name} = {tag.Description}");

        //        if (directory is XmpDirectory)
        //        {
        //            var xmps = (directory as XmpDirectory).GetXmpProperties();
        //            foreach (var xmp in xmps)
        //            {
        //                Console.WriteLine($"         {xmp.Key} = {xmp.Value}");
        //            }
        //        }
        //    }     
        //}
    }
}
