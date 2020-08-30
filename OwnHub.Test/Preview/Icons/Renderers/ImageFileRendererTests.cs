using OwnHub.File;
using OwnHub.File.Virtual;
using OwnHub.Preview.Icons;
using OwnHub.Preview.Icons.Renderers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OwnHub.Tests;

namespace OwnHub.Preview.Icons.Renderers.Tests
{
    [TestClass]
    public class ImageFileRendererTests
    {
        public TestContext TestContext { get; set; }
        

        public ImageFileRendererTests()
        {
        }

        public async Task RenderTestImageResourceIcon(string ResourceName, ImageFileRenderer Renderer, IconsRenderContext RenderContext)
        {
            IRegularFile file = TestUtils.ReadResourceRegularFile(ResourceName);
            await Renderer.Render(
                RenderContext,
                new DynamicIconsRenderInfo()
                {
                    file = file
                });
        }

        [TestMethod]
        public async Task TestRenderImageIcon()
        {
            IconsRenderContext RenderContext = new IconsRenderContext();
            var Renderer = new ImageFileRenderer();

            RenderContext.Resize(512, 512, false);

            await RenderTestImageResourceIcon("Test Image.png", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext);

            await RenderTestImageResourceIcon("20000px.png", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "Large Pixels");

            await RenderTestImageResourceIcon("EXIF Orientation Tag.jpg", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "EXIF Orientation Tag");

            await RenderTestImageResourceIcon("Grayscale With Alpha.png", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "Grayscale With Alpha");

            await RenderTestImageResourceIcon("Grayscale.jpg", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "Grayscale");
        }
    }
}
