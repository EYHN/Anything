using Anything.Preview.Mime;
using Anything.Preview.Mime.Schema;
using NUnit.Framework;

namespace Anything.Tests.Preview.Mime;

public class MimeTypeRulesTests
{
    [Test]
    public void FeatureTest()
    {
        var rules = MimeTypeRules.FromJson(
            "[{\"mime\":\"image/png\",\"extensions\":[\".png\"]},{\"mime\":\"image/jpeg\",\"extensions\":[\".jpg\",\".jpeg\",\".jpe\"]},{\"mime\":\"image/bmp\",\"extensions\":[ \".bmp\"]}]");

        Assert.AreEqual(MimeType.image_png, rules.Match("a.png"));
        Assert.AreEqual(MimeType.image_jpeg, rules.Match("a.jpg"));
        Assert.AreEqual(MimeType.image_jpeg, rules.Match("a.jpeg"));
        Assert.AreEqual(MimeType.image_jpeg, rules.Match("a.jpe"));
        Assert.AreEqual(MimeType.image_bmp, rules.Match("a.bmp"));
        Assert.AreEqual(null, rules.Match("a.txt"));

        Assert.Catch(() => MimeTypeRules.FromJson("{a:1}"));
        Assert.Catch(() => MimeTypeRules.FromJson(""));
    }
}
