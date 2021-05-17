using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.Preview;

namespace OwnHub.Tests.Preview
{
    internal static class TestUtilsRenderContextExtensions
    {
        public static async Task SaveTestResult(this RenderContext renderContext, string? name = null)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var resultName = name == null ? testName + ".png" : testName + " - " + name + ".png";

            await using var pngStream = renderContext.SnapshotPng().AsStream();

            await TestUtils.SaveResult(
                resultName,
                pngStream);
        }
    }
}
