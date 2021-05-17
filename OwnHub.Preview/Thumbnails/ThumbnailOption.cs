namespace OwnHub.Preview.Thumbnails
{
    public record ThumbnailOption(int Size)
    {
        public string ImageFormat { get; init; } = ThumbnailsConstants.DefaultImageFormat;
    }
}
