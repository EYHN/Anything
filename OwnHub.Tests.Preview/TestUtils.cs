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

        public static string GetTestDirectoryPath(string name)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            var dirname = Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName, name);
            Directory.CreateDirectory(dirname);
            Console.WriteLine("Create Test Directory: " + dirname);
            return dirname;
        }

        public static SqliteContext CreateSqliteContext(string name)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            Directory.CreateDirectory(
                Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName));
            var fileName = Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName, name + ".db");
            Console.WriteLine("Create Database File: " + fileName);
            return new SqliteContext(new SqliteConnectionProvider(fileName));
        }

        public static async Task SaveResult(string name, Stream data)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            Directory.CreateDirectory(
                Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName));
            var fileName = Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName, name);
            await using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                await data.CopyToAsync(output);
            }

            TestContext.AddTestAttachment(fileName);
            Console.WriteLine("Save Test Result: " + fileName);
        }
    }
}
