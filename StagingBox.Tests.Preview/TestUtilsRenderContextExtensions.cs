using System.Threading.Tasks;
using NUnit.Framework;
using StagingBox.Preview;

namespace StagingBox.Tests.Preview
{
    internal static class TestUtilsRenderContextExtensions
    {
        public static async Task SaveTestResult(this RenderContext renderContext, string name)
        {
            var resultName = name + ".png";

            await using var pngStream = renderContext.SnapshotPng().AsStream();

            await TestUtils.SaveResult(
                resultName,
                pngStream);
        }
    }
}
