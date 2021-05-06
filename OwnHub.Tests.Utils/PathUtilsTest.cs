using System;
using System.Collections.Generic;
using NUnit.Framework;
using OwnHub.Utils;

namespace OwnHub.Tests.Utils
{
    public class PathUtilsTest
    {
        [Test]
        public void JoinTest()
        {
            var joinTests = new Dictionary<string[], string>
            {
                { new[] { ".", "x/b", "..", "/b/c.js" }, "x/b/c.js" },
                { Array.Empty<string>(), "." },
                { new[] { "/.", "x/b", "..", "/b/c.js" }, "/x/b/c.js" },
                { new[] { "/foo", "../../../bar" }, "/bar" },
                { new[] { "foo", "../../../bar" }, "../../bar" },
                { new[] { "foo/", "../../../bar" }, "../../bar" },
                { new[] { "foo/x", "../../../bar" }, "../bar" },
                { new[] { "foo/x", "./bar" }, "foo/x/bar" },
                { new[] { "foo/x/", "./bar" }, "foo/x/bar" },
                { new[] { "foo/x/", ".", "bar" }, "foo/x/bar" },
                { new[] { "./" }, "./" },
                { new[] { ".", "./" }, "./" },
                { new[] { ".", ".", "." }, "." },
                { new[] { ".", "./", "." }, "." },
                { new[] { ".", "/./", "." }, "." },
                { new[] { ".", "/////./", "." }, "." },
                { new[] { "." }, "." },
                { new[] { "", "." }, "." },
                { new[] { "", "foo" }, "foo" },
                { new[] { "foo", "/bar" }, "foo/bar" },
                { new[] { "", "/foo" }, "/foo" },
                { new[] { "", "", "/foo" }, "/foo" },
                { new[] { "", "", "foo" }, "foo" },
                { new[] { "foo", "" }, "foo" },
                { new[] { "foo/", "" }, "foo/" },
                { new[] { "foo", "", "/bar" }, "foo/bar" },
                { new[] { "./", "..", "/foo" }, "../foo" },
                { new[] { "./", "..", "..", "/foo" }, "../../foo" },
                { new[] { ".", "..", "..", "/foo" }, "../../foo" },
                { new[] { "", "..", "..", "/foo" }, "../../foo" },
                { new[] { "/" }, "/" },
                { new[] { "/", "." }, "/" },
                { new[] { "/", ".." }, "/" },
                { new[] { "/", "..", ".." }, "/" },
                { new[] { "" }, "." },
                { new[] { "", "" }, "." },
                { new[] { " /foo" }, " /foo" },
                { new[] { " ", "foo" }, " /foo" },
                { new[] { " ", "." }, " " },
                { new[] { " ", "/" }, " /" },
                { new[] { " ", "" }, " " },
                { new[] { "/", "foo" }, "/foo" },
                { new[] { "/", "/foo" }, "/foo" },
                { new[] { "/", "//foo" }, "/foo" },
                { new[] { "/", "", "/foo" }, "/foo" },
                { new[] { "", "/", "foo" }, "/foo" },
                { new[] { "", "/", "/foo" }, "/foo" }
            };

            foreach (var (test, expected) in joinTests)
            {
                Assert.AreEqual(expected, PathUtils.Join(test));
            }
        }
    }
}
