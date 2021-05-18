using System.Threading.Tasks;

namespace StagingBox.Preview.Thumbnails.Renderers
{
    public interface IThumbnailsRenderer
    {
        public Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderOption option);

        public bool IsSupported(ThumbnailsRenderOption option);
    }
}
