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

namespace OwnHub.Preview.Icons.Renderers.Tests
{
    [TestClass]
    public class TextFileRendererTests
    {
        public TestContext TestContext { get; set; }

        IconsRenderContext RenderContext = new IconsRenderContext();
        IRegularFile file = TestUtils.ReadResourceRegularFile("Program.c");

        public TextFileRendererTests()
        {
        }

        [TestMethod]
        public async Task TestRenderTextIcon()
        {
            var renderer = new TextFileRenderer();
            await renderer.Render(
                RenderContext,
                new DynamicIconsRenderInfo()
                {
                    file = file
                });

            await RenderContext.SaveTestResult(TestContext);
        }
    }
}
