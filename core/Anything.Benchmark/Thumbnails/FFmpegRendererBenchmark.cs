using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
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
        private FileHandle _exampleMp4FileHandle = null!;

        [GlobalSetup]
        public async Task Setup()
        {
            _renderContext = new ThumbnailsRenderContext();
            _fileService = new FileService();
            _fileService.AddFileSystem(
                "test",
                new MemoryFileSystem());
            var root = await _fileService.CreateFileHandle(Url.Parse("file://test/"));
            _exampleMp4FileHandle = await _fileService.CreateFile(
                root,
                "example.mp4",
                Resources.ReadEmbeddedFile(typeof(FFmpegRendererBenchmark).Assembly, "/Resources/example.mp4"));
            _renderer = new FFmpegRenderer(_fileService);
            _renderOption = new ThumbnailsRenderOption { Size = 512 };
        }

        [Benchmark]
        public async Task FFmpeg() => await _renderer.Render(
            _renderContext,
            new ThumbnailsRenderFileInfo(_exampleMp4FileHandle, await _fileService.Stat(_exampleMp4FileHandle), MimeType.video_mp4),
            _renderOption);

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileService.Dispose();
        }
    }
}
