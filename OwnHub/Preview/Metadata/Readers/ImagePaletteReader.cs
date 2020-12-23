using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetVips;
using OwnHub.File;

namespace OwnHub.Preview.Metadata.Readers
{
    public class ImagePaletteReader: IMetadataReader
    {
        public string Name { get; } = "ImagePaletteReader";
        
        public static readonly string[] AllowMimeTypes =
        {
            "image/png", "image/jpeg", "image/bmp", "image/gif", "image/webp"
        };
        
        public bool IsSupported(IFile file)
        {
            if (file is IRegularFile)
                if (AllowMimeTypes.Contains(file.MimeType?.Mime))
                    return true;
            return false;
        }

        public async Task<MetadataEntry> ReadMetadata(IFile file, MetadataEntry metadata)
        {
            if (!IsSupported(file)) return metadata;
            
            Image sourceVipsImage;
            if (file is File.Local.RegularFile localFile)
                sourceVipsImage =
                    Image.Thumbnail(localFile.GetRealPath(), 112, 112, noRotate: false);
            else
                await using (Stream? stream = ((IRegularFile)file).Open())
                await using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);

                    byte[] data = ms.ToArray();

                    sourceVipsImage =
                        Image.ThumbnailBuffer(data, 112, height: 112, noRotate: false);
                }
            
            Palette.Palette palette = Palette.Palette.From(sourceVipsImage).Generate();
            
            var swatches = palette.GetSwatches();

            metadata.Palette = string.Join(", ", swatches
                .Select(swatch => swatch.GetRgb())
                .Select(color => "#" + color.ToString("X").Substring(2))
                .ToArray());

            return metadata;
        }
    }
}