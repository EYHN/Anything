using System.Threading.Tasks;

namespace OwnHub.Preview.Thumbnails.Renderers
{
    public interface IThumbnailsRenderer
    {
        public Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsIconsRenderOption option);

        public bool IsSupported(ThumbnailsIconsRenderOption option);
    }
}
