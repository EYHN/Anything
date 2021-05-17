using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Database;
using OwnHub.Database.Provider;

namespace OwnHub.Tests.Preview
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

        public static SqliteContext CreateSqliteContext([CallerMemberName] string callerMemberName = "sqlite")
        {
            var testName = Regex.Replace(
                TestContext.CurrentContext.Test.ClassName ?? TestContext.CurrentContext.Test.ID,
                "(\\w+\\.)*",
                string.Empty);
            Directory.CreateDirectory(
                Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, testName));
            var fileName = Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, testName, callerMemberName + ".db");
            Console.WriteLine("Create Database File: " + fileName);
            return new SqliteContext(new SqliteConnectionProvider(fileName));
        }

        public static async Task SaveResult(string name, Stream data)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            Directory.CreateDirectory(
                Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, testName));
            var fileName = Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, testName, name);
            await using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                await data.CopyToAsync(output);
            }

            TestContext.AddTestAttachment(fileName);
            Console.WriteLine("Save Test Result: " + fileName);
        }
    }
}
