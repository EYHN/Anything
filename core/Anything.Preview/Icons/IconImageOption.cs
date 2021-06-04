namespace Anything.Preview.Icons
{
    public record IconImageOption
    {
        public int Size { get; init; } = IconsConstants.DefaultSize;

        public string ImageFormat { get; init; } = IconsConstants.DefaultImageFormat;
    }
}
