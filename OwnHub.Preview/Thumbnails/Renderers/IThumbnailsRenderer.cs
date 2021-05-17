using System.Threading.Tasks;

namespace OwnHub.Preview.Thumbnails.Renderers
{
    public interface IThumbnailsRenderer
    {
        public Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderOption option);

        public bool IsSupported(ThumbnailsRenderOption option);
    }
}
