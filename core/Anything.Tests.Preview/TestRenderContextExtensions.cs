using System.Threading.Tasks;
using Anything.Preview;

namespace Anything.Tests.Preview;

internal static class TestRenderContextExtensions
{
    public static async Task SaveTestResult(this RenderContext renderContext, string name)
    {
        var resultName = name + ".png";

        var pngBuffer = renderContext.SnapshotPngBuffer();

        await TestUtils.SaveResult(
            resultName,
            pngBuffer);
    }
}
