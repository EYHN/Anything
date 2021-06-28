using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Anything.Tests.Search
{
    public class TestUtils
    {
        private static readonly string _resultDirectoryName = "TestResult-" + DateTime.UtcNow.ToString("yyyy-MM-dd\"T\"hh-mm-ss");

        public static string GetTestDirectoryPath([CallerMemberName] string callerMemberName = "test")
        {
            var testName = Regex.Replace(
                TestContext.CurrentContext.Test.ClassName ?? TestContext.CurrentContext.Test.ID,
                "(\\w+\\.)*",
                string.Empty);
            var dirname = Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, testName, callerMemberName);
            Directory.CreateDirectory(dirname);
            Console.WriteLine("Create Test Directory: " + dirname);
            return dirname;
        }
    }
}
