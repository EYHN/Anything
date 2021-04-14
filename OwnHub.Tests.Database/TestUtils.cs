using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OwnHub.Database;
using OwnHub.Database.Provider;

namespace OwnHub.Tests.Database
{
    public class TestUtils
    {
        private static readonly string _resultDirectoryName = "TestResult-" + DateTime.UtcNow.ToString("yyyy-MM-dd\"T\"hh-mm-ss");

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
    }
}
