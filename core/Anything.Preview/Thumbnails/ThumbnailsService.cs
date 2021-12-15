using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Property;
using Anything.Preview.Mime;
using Anything.Preview.Thumbnails.Renderers;
using Anything.Utils;
using SkiaSharp;

namespace Anything.Preview.Thumbnails;

public class ThumbnailsService
    : IThumbnailsService
{
    private readonly IFileService _fileService;

    private readonly IMimeTypeService _mimeType;

    private readonly ObjectPool<ThumbnailsRenderContext> _renderContextPool =
        new(Environment.ProcessorCount, () => new ThumbnailsRenderContext());

    private readonly ImmutableArray<IThumbnailsRenderer> _renderers;

    public ThumbnailsService(IFileService fileService, IMimeTypeService mimeType, IEnumerable<IThumbnailsRenderer> renderers)
    {
        _fileService = fileService;
        _mimeType = mimeType;
        _renderers = renderers.ToImmutableArray();
    }

    public async ValueTask<bool> IsSupportThumbnail(FileHandle fileHandle)
    {
        var stats = await _fileService.Stat(fileHandle);
        var mimeType = await _mimeType.GetMimeType(fileHandle);
        return _renderers.Any(renderer => renderer.IsSupported(new ThumbnailsRenderFileInfo(fileHandle, stats, mimeType)));
    }

    public async ValueTask<ThumbnailImage?> GetThumbnailImage(FileHandle fileHandle, ThumbnailOption option)
    {
        var targetSize = option.Size;
        var targetImageFormat = option.ImageFormat;
        if (targetImageFormat != "image/png")
        {
            throw new ArgumentException("Image format not support!");
        }

        var stats = await _fileService.Stat(fileHandle);

        var fileHash = stats.Hash;

        // Read the Cache
        var cachedThumbnail = await GetSizedCachedThumbnail(fileHandle, fileHash, targetSize, targetImageFormat) ??
                              await GetDefaultCachedThumbnail(fileHandle, fileHash);

        if (cachedThumbnail != null)
        {
            if (cachedThumbnail.Size == targetSize && cachedThumbnail.ImageFormat == targetImageFormat)
            {
                return cachedThumbnail;
            }

            using var bitmap = SKBitmap.Decode(cachedThumbnail.Data.Span);
            using var resizedBitmap = bitmap.Resize(new SKSizeI(targetSize, targetSize), SKFilterQuality.High);
            using var encodedData = resizedBitmap.Encode(SKEncodedImageFormat.Png, 100);

            var resizedThumbnail = new ThumbnailImage("image/png", targetSize, encodedData.ToArray());
            await CacheSizedThumbnail(fileHandle, fileHash, resizedThumbnail);

            return resizedThumbnail;
        }

        var mimeType = await _mimeType.GetMimeType(fileHandle);
        var fileInfo = new ThumbnailsRenderFileInfo(fileHandle, stats, mimeType);

        using var poolItem = await _renderContextPool.GetRefAsync();

        var ctx = poolItem.Value;
        ctx.Resize(targetSize, targetSize, false);

        var matchedRenderers = _renderers.Where(renderer => renderer.IsSupported(fileInfo));

        var renderOption = new ThumbnailsRenderOption { Size = option.Size };
        foreach (var renderer in matchedRenderers)
        {
            bool success;

            ctx.Save();
            try
            {
                success = await renderer.Render(ctx, fileInfo, renderOption);
            }
            finally
            {
                ctx.Restore();
            }

            if (!success)
            {
                continue;
            }

            if (ctx.Width != targetSize || ctx.Height != targetSize)
            {
                ctx.Resize(targetSize, targetSize);
            }

            var encodedData = ctx.SnapshotPngBuffer();

            var thumbnail = new ThumbnailImage("image/png", targetSize, encodedData);
            await CacheSizedThumbnail(fileHandle, fileHash, thumbnail);

            return thumbnail;
        }

        return null;
    }

    private async ValueTask<ThumbnailImage?> GetSizedCachedThumbnail(FileHandle fileHandle, FileHash fileHash, int size, string imageFormat)
    {
        return await _fileService.GetObjectProperty<ThumbnailImage>(
            fileHandle,
            $"thumbnail-cache-${fileHash.ContentTag}-${size}-${imageFormat}");
    }

    private async ValueTask CacheSizedThumbnail(FileHandle fileHandle, FileHash fileHash, ThumbnailImage thumbnailImage)
    {
        await _fileService.AddOrUpdateObjectProperty(
            fileHandle,
            $"thumbnail-cache-${fileHash.ContentTag}-${thumbnailImage.Size}-${thumbnailImage.ImageFormat}",
            thumbnailImage,
            PropertyFeature.AutoDeleteWhenFileUpdate);
    }

    private async ValueTask<ThumbnailImage?> GetDefaultCachedThumbnail(FileHandle fileHandle, FileHash fileHash)
    {
        return await _fileService.GetObjectProperty<ThumbnailImage>(fileHandle, $"thumbnail-cache-${fileHash}");
    }
}
