using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OwnHub.Preview.Metadata.Tests
{
    [TestClass]
    public class TagLibTests
    {
        //[TestMethod]
        //public void TestImageMetadataReader()
        //{
        //    var tfile = TagLib.File.Create(new FileBytesAbstraction("testimage.jpg", Resources._3316c8102211155_5f3183639a963));
        //    string title = tfile.Tag.Title;
        //    var tag = tfile.Tag as TagLib.Image.CombinedImageTag;
        //    DateTime? snapshot = tag.DateTime;
        //    Console.WriteLine("Title: {0}, snapshot taken on {1}", title, snapshot);
        //}

        //[TestMethod]
        //public void TestAudioMetadataReader()
        //{
        //    var tfile = TagLib.File.Create(new FileBytesAbstraction("testmusic.flac", Resources._01_Tell_Your_World));
        //    string title = tfile.Tag.Title;
        //    TimeSpan duration = tfile.Properties.Duration;
        //    Console.WriteLine("Title: {0}, duration: {1}", title, duration);
        //}

        public class FileBytesAbstraction : TagLib.File.IFileAbstraction
        {
            public FileBytesAbstraction(string name, byte[] bytes)
            {
                Name = name;

                var stream = new MemoryStream(bytes);
                ReadStream = stream;
                WriteStream = stream;
            }

            public void CloseStream(Stream stream)
            {
                stream.Dispose();
            }

            public string Name { get; private set; }

            public Stream ReadStream { get; private set; }

            public Stream WriteStream { get; private set; }
        }
    }
}
