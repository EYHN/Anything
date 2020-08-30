using OwnHub.File;
using OwnHub.File.Virtual;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwnHub.Preview;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace OwnHub.Tests
{
    public static class TestUtils
    {
        public static Stream ReadResourceStream(string name)
        {
            return new FileStream(Path.Join(GetApplicationRoot(), "Resources", name), FileMode.Open, FileAccess.Read);
        }

        public static byte[] ReadResource(string name)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream ReadStream = ReadResourceStream(name))
                {
                    ReadStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static IRegularFile ReadResourceRegularFile(string name)
        {
            return new File.Local.RegularFile(name, new FileInfo(Path.Join(GetApplicationRoot(), "Resources", name)));
        }

        public static async Task SaveResult(string Name, Stream Data, TestContext TestContext)
        {
            var ClassName = Regex.Replace(TestContext.FullyQualifiedTestClassName, ".*\\.", "");
            Directory.CreateDirectory(Path.Join(TestContext.TestResultsDirectory, ClassName));
            var FileName = Path.Join(TestContext.TestResultsDirectory, ClassName, Name);
            Stream output = new FileStream(FileName, FileMode.OpenOrCreate);
            await Data.CopyToAsync(output);
            TestContext.AddResultFile(FileName);
        }

        public static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }

    static class TestUtilsRenderContextExtensions
    {
        public static async Task SaveTestResult(this RenderContext RenderContext, TestContext TestContext, String Name = null)
        {
            string ResultName = Name == null ? TestContext.TestName + ".png" : TestContext.TestName + " - " + Name + ".png";

            await TestUtils.SaveResult(
                ResultName,
                RenderContext.SnapshotPNG().AsStream(),
                TestContext
                );
        }
    }
}
