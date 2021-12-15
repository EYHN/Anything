namespace Anything.Preview.Thumbnails;

public record ThumbnailOption
{
    public int Size { get; init; } = ThumbnailsConstants.DefaultSize;

    public string ImageFormat { get; init; } = ThumbnailsConstants.DefaultImageFormat;
}
