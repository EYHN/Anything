using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Mime.Schema;
using SkiaSharp;

namespace Anything.Preview.Thumbnails.Renderers;

public abstract class BaseThumbnailsRenderer : IThumbnailsRenderer
{
    private ImmutableArray<MimeType>? _cacheSupportMimeTypes;

    /// <summary>
    ///     Gets the mimetype supported by the renderer.
    /// </summary>
    protected abstract ImmutableArray<MimeType> SupportMimeTypes { get; }

    protected virtual long MaxFileSize => long.MaxValue;

    async Task<bool> IThumbnailsRenderer.Render(
        ThumbnailsRenderContext ctx,
        ThumbnailsRenderFileInfo fileInfo,
        ThumbnailsRenderOption option)
    {
        if (!IsSupported(fileInfo))
        {
            return false;
        }

        return await Render(ctx, fileInfo, option);
    }

    public virtual bool IsSupported(ThumbnailsRenderFileInfo fileInfo)
    {
        if (_cacheSupportMimeTypes == null)
        {
            _cacheSupportMimeTypes = SupportMimeTypes;
        }

        var fileMimeType = fileInfo.MimeType;

        if (fileInfo.Type.HasFlag(FileType.File) &&
            fileMimeType != null &&
            _cacheSupportMimeTypes.Value.Contains(fileMimeType) &&
            fileInfo.Size <= MaxFileSize)
        {
            return true;
        }

        return false;
    }

    protected abstract Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderFileInfo fileInfo, ThumbnailsRenderOption option);
}
