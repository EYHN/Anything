using SkiaSharp;

namespace Anything.Preview;

public interface ISkView
{
    public SKSize Size { get; }

    public void Draw(SKCanvas canvas, SKRect rect, SKPaint paint);
}

public class SkImageView : ISkView
{
    public SKImage Image { get; }

    public SKSize Size => Image.Info.Size;

    public SkImageView(SKImage image)
    {
        Image = image;
    }

    public void Draw(SKCanvas canvas, SKRect rect, SKPaint paint)
    {
        canvas.DrawImage(Image, rect, paint);
    }
}

public class SkPictureView : ISkView
{
    public SKPicture Picture { get; }

    public SKSize Size { get; }

    public SkPictureView(SKPicture picture, SKSize size)
    {
        Picture = picture;
        Size = size;
    }

    public void Draw(SKCanvas canvas, SKRect rect, SKPaint paint)
    {
        var matrix = new SKMatrix
        {
            ScaleX = Size.Width / rect.Width,
            ScaleY = Size.Height / rect.Height,
            TransX = rect.Left,
            TransY = rect.Top,
            Persp2 = 1f
        };
        canvas.DrawPicture(Picture, ref matrix, paint);
    }
}
