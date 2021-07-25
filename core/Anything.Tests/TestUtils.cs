using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Database.Provider;
using NUnit.Framework;

namespace Anything.Tests
{
    public static class TestUtils
    {
        private static readonly string _resultDirectoryName =
            "TestResult-" + DateTime.UtcNow.ToString("yyyy-MM-dd\"T\"hh-mm-ss", CultureInfo.InvariantCulture);

        public static string GetTestDirectoryPath(string? directoryName = null)
        {
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            var testName = TestContext.CurrentContext.Test.Name;
            var dirname = Path.Join(
                TestContext.CurrentContext.WorkDirectory,
                _resultDirectoryName,
                className,
                testName,
                directoryName ?? testName);

            Directory.CreateDirectory(dirname);
            Console.WriteLine("Create Test Directory: " + dirname);
            return dirname;
        }

        public static SqliteContext CreateSqliteContext(string? databaseName = null)
        {
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            var testName = TestContext.CurrentContext.Test.Name;
            Directory.CreateDirectory(
                Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName));
            var fileName = Path.Join(
                TestContext.CurrentContext.WorkDirectory,
                _resultDirectoryName,
                className,
                testName,
                (databaseName ?? testName) + ".db");
            Console.WriteLine("Create Database File: " + fileName);
            return new SqliteContext(new SqliteConnectionProvider(fileName));
        }

        public static async Task SaveResult(string name, Stream data)
        {
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            var testName = TestContext.CurrentContext.Test.Name;
            Directory.CreateDirectory(
                Path.Join(TestContext.CurrentContext.WorkDirectory, _resultDirectoryName, className, testName));
            var fileName = Path.Join(
                TestContext.CurrentContext.WorkDirectory,
                _resultDirectoryName,
                className,
                testName,
                name);
            await using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                await data.CopyToAsync(output);
            }

            TestContext.AddTestAttachment(fileName);
            Console.WriteLine("Save Test Result: " + fileName);
        }
    }
}
