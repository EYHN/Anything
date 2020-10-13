using OwnHub.File;
using OwnHub.File.Virtual;
using OwnHub.Preview;
using OwnHub.Preview.Icons;
using OwnHub.Preview.Icons.Renderers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OwnHub.Tests;
using System.Threading;

namespace OwnHub.Preview.Icons.Renderers.Tests
{
    [TestClass]
    public class TextFileRendererTests
    {
        public TestContext TestContext { get; set; }

        public TextFileRendererTests()
        {
        }

        public async Task RenderTestTextResourceIcon(string ResourceName, TextFileRenderer Renderer, IconsRenderContext RenderContext)
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
        public async Task TestRenderTextIcon()
        {
            IconsRenderContext RenderContext = new IconsRenderContext();
            var Renderer = new TextFileRenderer();

            RenderContext.Resize(1024, 1024, false);
            await RenderTestTextResourceIcon("Test Text.txt", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "1024w");

            RenderContext.Resize(512, 512, false);
            await RenderTestTextResourceIcon("Test Text.txt", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "512w");

            RenderContext.Resize(256, 256, false);
            await RenderTestTextResourceIcon("Test Text.txt", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "256w");

            RenderContext.Resize(128, 128, false);
            await RenderTestTextResourceIcon("Test Text.txt", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "128w");

            RenderContext.Resize(64, 64, false);
            await RenderTestTextResourceIcon("Test Text.txt", Renderer, RenderContext);
            await RenderContext.SaveTestResult(TestContext, "64w");
        }
    }
}
