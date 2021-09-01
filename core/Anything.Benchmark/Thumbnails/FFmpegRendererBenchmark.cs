using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.FileSystem.Provider;
using Anything.Preview.Mime.Schema;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace Anything.Benchmark.Thumbnails
{
    [SimpleJob]
    [RPlotExporter]
    [MemoryDiagnoser]
    public class FFmpegRendererBenchmark : Disposable
    {
        private ThumbnailsRenderContext _renderContext = null!;
        private FileService _fileService = null!;
        private FFmpegRenderer _renderer = null!;
        private ThumbnailsRenderOption _renderOption = null!;
        private readonly Url _exampleMp4 = Url.Parse("file://test/example.mp4");

        [GlobalSetup]
        public void Setup()
        {
            _renderContext = new ThumbnailsRenderContext();
            _fileService = new FileService();
            var mfs = new MemoryFileSystemProvider();
            mfs.WriteFile(
                _exampleMp4,
                Resources.ReadEmbeddedFile(typeof(FFmpegRendererBenchmark).Assembly, "/Resources/example.mp4")).AsTask().Wait();
            _fileService.AddFileSystem(
                Url.Parse("file://test/"),
                new ReadonlyStaticFileSystem(Url.Parse("file://test/"), mfs));
            _renderer = new FFmpegRenderer(_fileService);
            _renderOption = new ThumbnailsRenderOption { Size = 512 };
        }

        [Benchmark]
        public async Task FFmpeg() => await _renderer.Render(
            _renderContext,
            new ThumbnailsRenderFileInfo(_exampleMp4, await _fileService.Stat(_exampleMp4), MimeType.video_mp4),
            _renderOption);

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileService.Dispose();
        }
    }
}
