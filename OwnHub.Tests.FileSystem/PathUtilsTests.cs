using NUnit.Framework;
using OwnHub.FileSystem;

namespace OwnHub.Tests.FileSystem
{
    public class PathUtilsTests
    {
        [Test]
        public void ResolveTest()
        {
            Assert.AreEqual("/var/file", PathUtils.Resolve("/var/lib", "../", "file/"));
            Assert.AreEqual("/file", PathUtils.Resolve("/var/lib", "/../", "file/"));
            Assert.AreEqual("/", PathUtils.Resolve("a/b/c/", "../../.."));
            Assert.AreEqual("/", PathUtils.Resolve("."));
            Assert.AreEqual("/absolute", PathUtils.Resolve("/some/dir", ".", "/absolute/"));
            Assert.AreEqual("/foo/tmp.3/cycles/root.js", PathUtils.Resolve("/foo/tmp.3/", "../tmp.3/cycles/root.js"));
            Assert.AreEqual("/foo", PathUtils.Resolve("../foo"));
        }
    }
}
