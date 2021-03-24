using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.File;
using OwnHub.File.Local;
using OwnHub.Preview;
using Directory = System.IO.Directory;

namespace OwnHub.Tests
{
    public static class TestUtils
    {
        private const string ResultDirectoryName = "TestResult";

        public static Stream ReadResourceStream(string name)
        {
            return new FileStream(Path.Join(GetApplicationRoot(), "Resources", name), FileMode.Open, FileAccess.Read);
        }

        public static byte[] ReadResource(string name)
        {
            using var ms = new MemoryStream();
            using var readStream = ReadResourceStream(name);

            readStream.CopyTo(ms);
            return ms.ToArray();
        }

        public static IRegularFile OpenResourceRegularFile(string name)
        {
            return new RegularFile(name, new FileInfo(Path.Join(GetApplicationRoot(), "Resources", name)));
        }

        public static IDirectory OpenResourceDirectory(string name)
        {
            return new OwnHub.File.Local.Directory(name, new DirectoryInfo(Path.Join(GetApplicationRoot(), "Resources", name)));
        }

        public static async Task SaveResult(string name, Stream data)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            Directory.CreateDirectory(
                Path.Join(TestContext.CurrentContext.WorkDirectory, ResultDirectoryName, testName));
            var fileName = Path.Join(TestContext.CurrentContext.WorkDirectory, ResultDirectoryName, testName, name);
            await using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                await data.CopyToAsync(output);
            }

            TestContext.AddTestAttachment(fileName);
            Console.WriteLine("Save Test Result: " + fileName);
        }

        public static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        }
    }

#pragma warning disable SA1402
    internal static class TestUtilsRenderContextExtensions
    {
        public static async Task SaveTestResult(this RenderContext renderContext, string? name = null)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var resultName = name == null ? testName + ".png" : testName + " - " + name + ".png";

            await using var pngStream = renderContext.SnapshotPng().AsStream();

            await TestUtils.SaveResult(
                resultName,
                pngStream);
        }
    }
#pragma warning restore SA1402
}