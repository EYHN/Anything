namespace Anything.Preview.Thumbnails.Renderers
{
    public record ThumbnailsRenderOption
    {
        public int Size { get; init; } = ThumbnailsConstants.DefaultSize;
    }
}
