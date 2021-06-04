using System;
using System.IO;
using NUnit.Framework;

namespace Anything.Tests.Server
{
    public class TestUtils
    {
        private static readonly string _resultDirectoryName = "TestResult-" + DateTime.UtcNow.ToString("yyyy-MM-dd\"T\"hh-mm-ss");

        public static string GetTestDirectoryPath(string name)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            var dirname = Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName, name);
            Directory.CreateDirectory(dirname);
            Console.WriteLine("Create Test Directory: " + dirname);
            return dirname;
        }
    }
}
