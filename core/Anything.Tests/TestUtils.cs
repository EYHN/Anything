using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Database.Provider;
using Anything.Utils.Logging;
using NUnit.Framework;

namespace Anything.Tests
{
    public static class TestUtils
    {
        private static readonly string _resultDirectory = InitializeResultDirectory();

        public static ILogger Logger => new Logger(new SerilogCommandLineLoggerBackend());

        private static string InitializeResultDirectory()
        {
            var retryCount = 0;
            var dateText = DateTime.UtcNow.ToString("yyyy-MM-dd\"T\"hh-mm-ss", CultureInfo.InvariantCulture);

            while (retryCount < 10)
            {
                var resultDirectoryName = $"TestResult-{dateText}" + (retryCount > 0 ? $"-{retryCount}" : "");
                var resultDirectoryPath = Path.Join(TestContext.CurrentContext.WorkDirectory, resultDirectoryName);
                if (!File.Exists(resultDirectoryPath) && !Directory.Exists(resultDirectoryPath))
                {
                    Directory.CreateDirectory(resultDirectoryPath);
                    return resultDirectoryPath;
                }

                retryCount++;
            }

            throw new InvalidOperationException("Can't create test result directory.");
        }

        public static string GetTestDirectoryPath(string? directoryName = null)
        {
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            if (className == "TestExecutionContext+AdhocContext")
            {
                className = "";
            }

            var testName = TestContext.CurrentContext.Test.Name;
            if (testName == "AdhocTestMethod")
            {
                testName = "";
            }

            var dirname = Path.Join(
                _resultDirectory,
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
                Path.Join(_resultDirectory, className, testName));
            var fileName = Path.Join(
                _resultDirectory,
                className,
                testName,
                (databaseName ?? testName) + ".db");
            Console.WriteLine("Create Database File: " + fileName);
            return new SqliteContext(new SqliteConnectionProvider(fileName));
        }

        public static async Task SaveResult(string name, ReadOnlyMemory<byte> data)
        {
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            var testName = TestContext.CurrentContext.Test.Name;
            Directory.CreateDirectory(
                Path.Join(_resultDirectory, className, testName));
            var fileName = Path.Join(
                _resultDirectory,
                className,
                testName,
                name);
            await using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                await output.WriteAsync(data);
            }

            TestContext.AddTestAttachment(fileName);
            Console.WriteLine("Save Test Result: " + fileName);
        }

        public static async Task SaveResult(string name, Stream data)
        {
            var className = TestContext.CurrentContext.Test.ClassName!.Split(".")[^1];
            var testName = TestContext.CurrentContext.Test.Name;
            Directory.CreateDirectory(
                Path.Join(_resultDirectory, className, testName));
            var fileName = Path.Join(
                _resultDirectory,
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
