using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.File;

namespace OwnHub.Tests.File
{
    [TestFixture]
    public class WalkerTests
    {
        [Test]
        public async Task FileWalkerTest()
        {
            IDirectory directory = TestUtils.OpenResourceDirectory("folder");

            await foreach (var file in new Walker(directory))
            {
                Console.WriteLine(file.Path);
            }
        }
    }
}