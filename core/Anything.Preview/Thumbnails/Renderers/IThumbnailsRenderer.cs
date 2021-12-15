using System.Threading.Tasks;

namespace Anything.Preview.Thumbnails.Renderers;

public interface IThumbnailsRenderer
{
    public Task<bool> Render(ThumbnailsRenderContext ctx, ThumbnailsRenderFileInfo fileInfo, ThumbnailsRenderOption option);

    public bool IsSupported(ThumbnailsRenderFileInfo fileInfo);
}
