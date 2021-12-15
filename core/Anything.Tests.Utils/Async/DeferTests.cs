using System.Threading.Tasks;
using Anything.Utils.Async;
using NUnit.Framework;

namespace Anything.Tests.Utils.Async;

public class DeferTests
{
    [Test]
    public async Task DeferTest()
    {
        var defer = new Defer();

        Assert.AreEqual(false, defer.IsCompleted);

        defer.Resolve();

        await defer.Wait();

        Assert.AreEqual(true, defer.IsCompleted);
    }
}
