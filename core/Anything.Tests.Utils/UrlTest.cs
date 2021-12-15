using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Utils;

public class UrlTest
{
    [Test]
    public void UrlFeatureTest()
    {
        {
            Assert.AreEqual(
                "https://www.eyhn.in/my/path",
                new Url("https", "www.eyhn.in", "/my/path", "", "").ToString());
            Assert.AreEqual(
                "https://www.eyhn.in/my/path",
                new Url("https", "www.eyhn.in", "/my/path", "", "").ToString());
            Assert.AreEqual(
                "https://www.eyhn.in/my/path",
                new Url("https", "www.EYHN.in", "/my/path", "", "").ToString());
            Assert.AreEqual(
                "https:///my/path",
                new Url("https", "", "my/path", "", "").ToString());
            Assert.AreEqual(
                "https:///my/path",
                new Url("https", "", "/my/path", "", "").ToString());
            Assert.AreEqual(
                "https://a-test-site.com/my/path?test%3Dtrue",
                new Url("https", "a-test-site.com", "my/path", "test=true", "").ToString());
            Assert.AreEqual(
                "https://a-test-site.com/my/path#test%3Dtrue",
                new Url("https", "a-test-site.com", "my/path", "", "test=true").ToString());
        }

        {
            var value = Url.Parse("file://shares/pröjects/c%23/#l12");
            Assert.AreEqual(value.Authority, "shares");
            Assert.AreEqual(value.Path, "/pröjects/c#/");
            Assert.AreEqual(value.Fragment, "l12");
            Assert.AreEqual(value.ToString(), "file://shares/pr%C3%B6jects/c%23/#l12");

            var value2 = Url.Parse(value.ToString());
            Assert.AreEqual(value.Authority, value2.Authority);
            Assert.AreEqual(value.Path, value2.Path);
            Assert.AreEqual(value.Query, value2.Query);
            Assert.AreEqual(value.Fragment, value2.Fragment);
        }

        {
            var uri = Url.Parse("foo:bar/path");
            var uri2 = uri with { Scheme = "foo", Path = "bar/path" };
            Assert.IsTrue(uri == uri2);
        }

        {
            var uri = Url.Parse("foo:bar/path");
            Assert.Catch(
                () => uri = uri with { Scheme = "fai:l" });
            Assert.Catch(
                () => uri = uri with { Scheme = "fäil" });
            Assert.Catch(
                () => uri = uri with { Path = "//fail" });
        }

        {
            var value = Url.Parse("http:/api/files/test.me?t=1234");
            Assert.AreEqual(value.Scheme, "http");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/api/files/test.me");
            Assert.AreEqual(value.Query, "t=1234");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("http://api/files/test.me?t=1234");
            Assert.AreEqual(value.Scheme, "http");
            Assert.AreEqual(value.Authority, "api");
            Assert.AreEqual(value.Path, "/files/test.me");
            Assert.AreEqual(value.Query, "t=1234");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("file:///c:/test/me");
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/c:/test/me");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("file://shares/files/c%23/p.cs");
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "shares");
            Assert.AreEqual(value.Path, "/files/c#/p.cs");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse(
                "file:///c:/Source/Z%C3%BCrich%20or%20Zurich%20(%CB%88zj%CA%8A%C9%99r%C9%AAk,/Code/resources/app/plugins/c%23/plugin.json");
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/c:/Source/Zürich or Zurich (ˈzjʊərɪk,/Code/resources/app/plugins/c#/plugin.json");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("file:///c:/test %25/path");
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/c:/test %/path");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("inmemory:");
            Assert.AreEqual(value.Scheme, "inmemory");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("foo:api/files/test");
            Assert.AreEqual(value.Scheme, "foo");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/api/files/test");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("file:?q");
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/");
            Assert.AreEqual(value.Query, "q");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("file:#d");
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "d");

            value = Url.Parse("f3ile:#d");
            Assert.AreEqual(value.Scheme, "f3ile");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "d");

            value = Url.Parse("foo+bar:path");
            Assert.AreEqual(value.Scheme, "foo+bar");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/path");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("foo-bar:path");
            Assert.AreEqual(value.Scheme, "foo-bar");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/path");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");

            value = Url.Parse("foo.bar:path");
            Assert.AreEqual(value.Scheme, "foo.bar");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/path");
            Assert.AreEqual(value.Query, "");
            Assert.AreEqual(value.Fragment, "");
        }

        {
            Assert.Catch(() => Url.Parse("file:////shares/files/p.cs"));
        }

        {
            var value = Url.Parse("file:///a.file");
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/a.file");
            Assert.AreEqual(value.ToString(), "file:///a.file");

            value = Url.Parse(value.ToString());
            Assert.AreEqual(value.Scheme, "file");
            Assert.AreEqual(value.Authority, "");
            Assert.AreEqual(value.Path, "/a.file");
            Assert.AreEqual(value.ToString(), "file:///a.file");
        }

        {
            var value = Url.Parse("https://foo:bar@localhost/far");
            Assert.AreEqual(value.ToString(), "https://foo:bar@localhost/far");

            value = Url.Parse("https://foo@localhost/far");
            Assert.AreEqual(value.ToString(), "https://foo@localhost/far");

            value = Url.Parse("https://foo:bAr@localhost:8080/far");
            Assert.AreEqual(value.ToString(), "https://foo:bAr@localhost:8080/far");

            value = Url.Parse("https://foo@localhost:8080/far");
            Assert.AreEqual(value.ToString(), "https://foo@localhost:8080/far");

            value = new Url("https", "föö:bör@löcalhost:8080", "/far", "", "");
            Assert.AreEqual(value.ToString(), "https://f%C3%B6%C3%B6:b%C3%B6r@l%C3%B6calhost:8080/far");
        }

        {
            Assert.AreEqual("file:///bazz", Url.Parse("file:///foo/").JoinPath("../../bazz").ToString());
            Assert.AreEqual("file:///bazz", Url.Parse("file:///foo").JoinPath("../../bazz").ToString());
            Assert.AreEqual("file:///bazz", Url.Parse("file:///foo").JoinPath("../../bazz").ToString());
            Assert.AreEqual("file:///foo/bar/bazz", Url.Parse("file:///foo/bar/").JoinPath("./bazz").ToString());
            Assert.AreEqual("file:///foo/bar/bazz", Url.Parse("file:///foo/bar").JoinPath("./bazz").ToString());
            Assert.AreEqual("file:///foo/bar/bazz", Url.Parse("file:///foo/bar").JoinPath("bazz").ToString());

            Assert.AreEqual("file:///bazz", Url.Parse("file:").JoinPath("bazz").ToString());
            Assert.AreEqual("https://domain/bazz", Url.Parse("https://domain").JoinPath("bazz").ToString());
            Assert.AreEqual("https:///bazz", Url.Parse("https:").JoinPath("bazz").ToString());
            Assert.AreEqual("https:///bazz", Url.Parse("https:").JoinPath("bazz").ToString());
        }

        Assert.AreEqual("bar.txt", Url.Parse("file:///example/foo/bar.txt").Basename());
        Assert.AreEqual("file:///example/foo", Url.Parse("file:///example/foo/bar.txt").Dirname().ToString());
        Assert.AreEqual(true, Url.Parse("file:///example/foo/bar.txt").StartsWith(Url.Parse("file:///example/foo")));
        Assert.AreEqual(false, Url.Parse("file:///example/foo/bar.txt").StartsWith(Url.Parse("memory:///example/foo")));
    }
}
