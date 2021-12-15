using System;
using System.Collections.Generic;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Utils;

public class PathLibTest
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
            Assert.AreEqual(expected, PathLib.Join(test));
        }
    }

    [Test]
    public void ResolveTest()
    {
        Assert.AreEqual("/var/file", PathLib.Resolve("/var/lib", "../", "file/"));
        Assert.AreEqual("/var/file", PathLib.Resolve("/var/lib", "../", "", "file/"));
        Assert.AreEqual("/file", PathLib.Resolve("/var/lib", "/../", "file/"));
        Assert.AreEqual("/", PathLib.Resolve("a/b/c/", "../../.."));
        Assert.AreEqual("/", PathLib.Resolve("."));
        Assert.AreEqual("/absolute", PathLib.Resolve("/some/dir", ".", "/absolute/"));
        Assert.AreEqual("/foo/tmp.3/cycles/root.js", PathLib.Resolve("/foo/tmp.3/", "../tmp.3/cycles/root.js"));
        Assert.AreEqual("/foo", PathLib.Resolve("../foo"));
    }

    [Test]
    public void NormalizeTest()
    {
        Assert.AreEqual("fixtures/b/c.js", PathLib.Normalize("./fixtures///b/../b/c.js"));
        Assert.AreEqual("/bar", PathLib.Normalize("/foo/../../../bar"));
        Assert.AreEqual("a/b", PathLib.Normalize("a//b//../b"));
        Assert.AreEqual("a/b/c", PathLib.Normalize("a//b//./c"));
        Assert.AreEqual("a/b", PathLib.Normalize("a//b//."));
        Assert.AreEqual("/x/y/z", PathLib.Normalize("/a/b/c/../../../x/y/z"));
        Assert.AreEqual("/foo/bar", PathLib.Normalize("///..//./foo/.//bar"));
        Assert.AreEqual("bar/", PathLib.Normalize("bar/foo../../"));
        Assert.AreEqual("bar", PathLib.Normalize("bar/foo../.."));
        Assert.AreEqual("bar/baz", PathLib.Normalize("bar/foo../../baz"));
        Assert.AreEqual("bar/foo../", PathLib.Normalize("bar/foo../"));
        Assert.AreEqual("bar/foo..", PathLib.Normalize("bar/foo.."));
        Assert.AreEqual("../../bar", PathLib.Normalize("../foo../../../bar"));
        Assert.AreEqual("../../bar", PathLib.Normalize("../.../.././.../../../bar"));
        Assert.AreEqual("../../../../../bar", PathLib.Normalize("../../../foo/../../../bar"));
        Assert.AreEqual("../../../../../../", PathLib.Normalize("../../../foo/../../../bar/../../"));
        Assert.AreEqual("../../", PathLib.Normalize("../foobar/barfoo/foo/../../../bar/../../"));
        Assert.AreEqual("../../../../baz", PathLib.Normalize("../.../../foobar/../../../bar/../../baz"));
        Assert.AreEqual("foo/bar\\baz", PathLib.Normalize("foo/bar\\baz"));
    }

    [Test]
    public void BasenameTest()
    {
        Assert.AreEqual("test-path-basename.js", PathLib.Basename("/fixtures/test/test-path-basename.js"));
        Assert.AreEqual(".js", PathLib.Basename(".js"));
        Assert.AreEqual("", PathLib.Basename(""));
        Assert.AreEqual("basename.ext", PathLib.Basename("/dir/basename.ext"));
        Assert.AreEqual("basename.ext", PathLib.Basename("/basename.ext"));
        Assert.AreEqual("basename.ext", PathLib.Basename("basename.ext"));
        Assert.AreEqual("basename.ext", PathLib.Basename("basename.ext/"));
        Assert.AreEqual("basename.ext", PathLib.Basename("basename.ext//"));
        Assert.AreEqual("bbb", PathLib.Basename("/aaa/bbb"));
        Assert.AreEqual("aaa", PathLib.Basename("/aaa/"));
        Assert.AreEqual("b", PathLib.Basename("/aaa/b"));
        Assert.AreEqual("b", PathLib.Basename("/a/b"));
        Assert.AreEqual("a", PathLib.Basename("//a"));

        Assert.AreEqual("\\dir\\basename.ext", PathLib.Basename("\\dir\\basename.ext"));
        Assert.AreEqual("\\basename.ext", PathLib.Basename("\\basename.ext"));
        Assert.AreEqual("basename.ext", PathLib.Basename("basename.ext"));
        Assert.AreEqual("basename.ext\\", PathLib.Basename("basename.ext\\"));
        Assert.AreEqual("basename.ext\\\\", PathLib.Basename("basename.ext\\\\"));
        Assert.AreEqual("foo", PathLib.Basename("foo"));
    }

    [Test]
    public void ExtnameTest()
    {
        var extnameTests = new[]
        {
            new[] { "", "" }, new[] { "/path/to/file", "" }, new[] { "/path/to/file.ext", ".ext" },
            new[] { "/path.to/file.ext", ".ext" }, new[] { "/path.to/file", "" }, new[] { "/path.to/.file", "" },
            new[] { "/path.to/.file.ext", ".ext" }, new[] { "/path/to/f.ext", ".ext" }, new[] { "/path/to/..ext", ".ext" },
            new[] { "/path/to/..", "" }, new[] { "file", "" }, new[] { "file.ext", ".ext" }, new[] { ".file", "" },
            new[] { ".file.ext", ".ext" }, new[] { "/file", "" }, new[] { "/file.ext", ".ext" }, new[] { "/.file", "" },
            new[] { "/.file.ext", ".ext" }, new[] { ".path/file.ext", ".ext" }, new[] { "file.ext.ext", ".ext" },
            new[] { "file.", "." }, new[] { ".", "" }, new[] { "./", "" }, new[] { ".file.ext", ".ext" }, new[] { ".file", "" },
            new[] { ".file.", "." }, new[] { ".file..", "." }, new[] { "..", "" }, new[] { "../", "" }, new[] { "..file.ext", ".ext" },
            new[] { "..file", ".file" }, new[] { "..file.", "." }, new[] { "..file..", "." }, new[] { "...", "." },
            new[] { "...ext", ".ext" }, new[] { "....", "." }, new[] { "file.ext/", ".ext" }, new[] { "file.ext//", ".ext" },
            new[] { "file/", "" }, new[] { "file//", "" }, new[] { "file./", "." }, new[] { "file.//", "." }, new[] { ".\\", "" },
            new[] { "..\\", ".\\" }, new[] { "file.ext\\", ".ext\\" }, new[] { "file.ext\\\\", ".ext\\\\" }, new[] { "file\\", "" },
            new[] { "file.\\", ".\\" }, new[] { "file.\\\\", ".\\\\" }
        };

        foreach (var test in extnameTests)
        {
            Assert.AreEqual(test[1], PathLib.Extname(test[0]));
        }
    }

    [Test]
    public void DirnameTest()
    {
        Assert.AreEqual("/a", PathLib.Dirname("/a/b/"));
        Assert.AreEqual("/a", PathLib.Dirname("/a/b"));
        Assert.AreEqual("/", PathLib.Dirname("/a"));
        Assert.AreEqual(".", PathLib.Dirname(""));
        Assert.AreEqual("/", PathLib.Dirname("/"));
        Assert.AreEqual("/", PathLib.Dirname("////"));
        Assert.AreEqual("//", PathLib.Dirname("//a"));
        Assert.AreEqual(".", PathLib.Dirname("foo"));
    }
}
