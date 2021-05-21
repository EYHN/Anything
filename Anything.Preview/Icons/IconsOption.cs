namespace Anything.Preview.Icons
{
    public record IconsOption
    {
        public int Size { get; init; } = IconsConstants.DefaultSize;

        public string ImageFormat { get; init; } = IconsConstants.DefaultImageFormat;
    }
}
