using System.Threading.Tasks;
using NUnit.Framework;
using OwnHub.File;
using OwnHub.Preview.Icons;
using OwnHub.Preview.Icons.Renderers;

namespace OwnHub.Tests.Preview.Icons.Renderers
{
    [TestFixture]
    public class ImageFileRendererTests
    {
        private static async Task RenderTestImageResourceIcon(string resourceName, ImageFileRenderer renderer,
            IconsRenderContext renderContext)
        {
            IRegularFile? file = TestUtils.ReadResourceRegularFile(resourceName);
            await renderer.Render(
                renderContext,
                new DynamicIconsRenderInfo(file)
            );
        }

        [Test]
        public async Task TestRenderImageIcon()
        {
            var renderContext = new IconsRenderContext();
            var renderer = new ImageFileRenderer();

            renderContext.Resize(512, 512, false);
            await RenderTestImageResourceIcon("Test Image.png", renderer, renderContext);
            await renderContext.SaveTestResult();

            renderContext.Resize(1024, 1024, false);
            await RenderTestImageResourceIcon("Test Image.png", renderer, renderContext);
            await renderContext.SaveTestResult("1024w");

            renderContext.Resize(256, 256, false);
            await RenderTestImageResourceIcon("Test Image.png", renderer, renderContext);
            await renderContext.SaveTestResult("256w");

            renderContext.Resize(128, 128, false);
            await RenderTestImageResourceIcon("Test Image.png", renderer, renderContext);
            await renderContext.SaveTestResult("128w");

            renderContext.Resize(512, 512, false);
            await RenderTestImageResourceIcon("20000px.png", renderer, renderContext);
            await renderContext.SaveTestResult("Large Pixels");

            await RenderTestImageResourceIcon("EXIF Orientation Tag.jpg", renderer, renderContext);
            await renderContext.SaveTestResult("EXIF Orientation Tag");

            await RenderTestImageResourceIcon("Grayscale With Alpha.png", renderer, renderContext);
            await renderContext.SaveTestResult("Grayscale With Alpha");

            await RenderTestImageResourceIcon("Grayscale.jpg", renderer, renderContext);
            await renderContext.SaveTestResult("Grayscale");
        }
    }
}