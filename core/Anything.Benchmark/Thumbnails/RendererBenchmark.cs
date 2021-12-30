using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Singleton;
using Anything.Preview.Mime.Schema;
using Anything.Preview.Thumbnails;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Anything.Benchmark.Thumbnails;

#pragma warning disable IDISP003
#pragma warning disable CA1001

[SimpleJob]
[RPlotExporter]
[MemoryDiagnoser]
public class RendererBenchmark
{
    private ServiceProvider _serviceProvider = null!;

    private FileHandle _examplePngFileHandle = null!;
    private FileStats _examplePngFileStats = null!;

    private FileHandle _exampleMp4FileHandle = null!;
    private FileStats _exampleMp4FileStats = null!;

    private FileHandle _exampleMp3FileHandle = null!;
    private FileStats _exampleMp3FileStats = null!;

    private FileHandle _exampleMp3WithCoverFileHandle = null!;
    private FileStats _exampleMp3WithCoverFileStats = null!;

    private IFileService _fileService = null!;
    private ThumbnailsRenderContext _renderContext = null!;
    private AudioFileRenderer _audioFileRenderer = null!;
    private VideoFileRenderer _videoFileRenderer = null!;
    private ThumbnailsRenderOption _renderOption = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<ILogger, NullLogger>();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.TryAddSingletonFileService(builder => builder.TryAddMemoryFileSystem("memory"));

        _serviceProvider = services.BuildServiceProvider();
        _fileService = _serviceProvider.GetRequiredService<IFileService>();

        var root = await _fileService.CreateFileHandle(Url.Parse("file://memory/"));
        _exampleMp4FileHandle = await _fileService.CreateFile(
            root,
            "example.mp4",
            Resources.ReadEmbeddedFile(typeof(RendererBenchmark).Assembly, "/Resources/example.mp4"));
        _exampleMp4FileStats = await _fileService.Stat(_exampleMp4FileHandle);
        _exampleMp3FileHandle = await _fileService.CreateFile(
            root,
            "example.mp3",
            Resources.ReadEmbeddedFile(typeof(RendererBenchmark).Assembly, "/Resources/example.mp3"));
        _exampleMp3FileStats = await _fileService.Stat(_exampleMp3FileHandle);

        _exampleMp3WithCoverFileHandle = await _fileService.CreateFile(
            root,
            "example-with-cover.mp3",
            Resources.ReadEmbeddedFile(typeof(RendererBenchmark).Assembly, "/Resources/example-with-cover.mp3"));
        _exampleMp3WithCoverFileStats = await _fileService.Stat(_exampleMp3WithCoverFileHandle);

        _examplePngFileHandle = await _fileService.CreateFile(
            root,
            "example.png",
            Resources.ReadEmbeddedFile(typeof(RendererBenchmark).Assembly, "/Resources/example.png"));
        _examplePngFileStats = await _fileService.Stat(_examplePngFileHandle);

        _renderContext = new ThumbnailsRenderContext();
        _audioFileRenderer = new AudioFileRenderer(_fileService);
        _videoFileRenderer = new VideoFileRenderer(_fileService);
        _renderOption = new ThumbnailsRenderOption { Size = 512 };
    }

    [Benchmark]
    public async Task RenderVideoFile()
    {
        await _videoFileRenderer.Render(
            _renderContext,
            new ThumbnailsRenderFileInfo(_exampleMp4FileHandle, _exampleMp4FileStats, MimeType.video_mp4),
            _renderOption);
    }

    [Benchmark]
    public async Task RenderAudioFile()
    {
        await _audioFileRenderer.Render(
            _renderContext,
            new ThumbnailsRenderFileInfo(_exampleMp3FileHandle, _exampleMp3FileStats, MimeType.audio_mpeg),
            _renderOption);
    }

    [Benchmark]
    public async Task RenderAudioWithCoverFile()
    {
        await _audioFileRenderer.Render(
            _renderContext,
            new ThumbnailsRenderFileInfo(_exampleMp3WithCoverFileHandle, _exampleMp3WithCoverFileStats, MimeType.audio_mpeg),
            _renderOption);
    }

    [Benchmark]
    public async Task RenderPngFile()
    {
        await _audioFileRenderer.Render(
            _renderContext,
            new ThumbnailsRenderFileInfo(_examplePngFileHandle, _examplePngFileStats, MimeType.image_png),
            _renderOption);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceProvider.Dispose();
        _renderContext.Dispose();
    }
}
