using System;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Preview.Thumbnails.Renderers
{
    public abstract class BaseThumbnailsRenderer : IThumbnailsRenderer
    {
        /// <summary>
        ///     Gets the mimetype supported by the renderer.
        /// </summary>
        protected abstract string[] SupportMimeTypes { get; }

        protected virtual long MaxFileSize => long.MaxValue;

        async Task<bool> IThumbnailsRenderer.Render(ThumbnailsRenderContext ctx, ThumbnailsRenderFileInfo fileInfo, ThumbnailsRenderOption option)
        {
            if (!IsSupported(fileInfo))
            {
                return false;
            }

            return await Render(ctx, fileInfo, option);
        }

        public virtual bool IsSupported(ThumbnailsRenderFileInfo fileInfo)
        {
            if (fileInfo.Type.HasFlag(FileType.File) && SupportMimeTypes.Contains(fileInfo.MimeType) && fileInfo.Size <= MaxFileSize)
            {
                return true;
            }

            return false;
        }

        public abstract Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderFileInfo fileInfo, ThumbnailsRenderOption option);
    }
}
