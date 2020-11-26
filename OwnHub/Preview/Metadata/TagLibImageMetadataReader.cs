namespace OwnHub.Preview.Metadata
{
    // public class TagLibImageMetadataReader
    // {
    //     public static readonly string[] AllowMimeTypes = new string[]
    //     {
    //         "image/png", "image/jpeg", "image/bmp", "image/git", "image/webp"
    //     };
    //
    //     // The Taglib uses the extension to identify the file format,
    //     // we need to re-standardize the file extension in this class.
    //     static readonly string[] MimeTypesExtensionName = new string[]
    //     {
    //         ".png", ".jpg", ".bmp", ".git", ".webp"
    //     };
    //
    //     public bool FileFilter(IFile file)
    //     {
    //         if (file is IRegularFile)
    //         {
    //             if (AllowMimeTypes.Contains(file.MimeType?.Mime))
    //             {
    //                 return true;
    //             }
    //         }
    //         return false;
    //     }
    //
    //     string VendorName(TagTypes vendor)
    //     {
    //         switch (vendor)
    //         {
    //             case TagTypes.Xiph:
    //                 return "Xiph";
    //             case TagTypes.Id3v1:
    //                 return "Id3v1";
    //             case TagTypes.Id3v2:
    //                 return "Id3v2";
    //             case TagTypes.Ape:
    //                 return "APE";
    //             case TagTypes.Apple:
    //                 return "Apple";
    //             case TagTypes.Asf:
    //                 return "ASF";
    //             case TagTypes.RiffInfo:
    //                 return "RiffInfo";
    //             case TagTypes.MovieId:
    //                 return "MovieId";
    //             case TagTypes.DivX:
    //                 return "DivX";
    //             case TagTypes.FlacMetadata:
    //                 return "FlacMetadata";
    //             case TagTypes.TiffIFD:
    //                 return "TiffIFD";
    //             case TagTypes.XMP:
    //                 return "XMP";
    //             case TagTypes.JpegComment:
    //                 return "JpegComment";
    //             case TagTypes.GifComment:
    //                 return "GifComment";
    //             case TagTypes.Png:
    //                 return "Png";
    //             case TagTypes.IPTCIIM:
    //                 return "IPTCIIM";
    //             case TagTypes.AudibleMetadata:
    //                 return "AudibleMetadata";
    //             case TagTypes.Matroska:
    //                 return "Matroska";
    //         }
    //         return null;
    //     }
    //
    //     public void ReadImageMetadata(IFile File)
    //     {
    //         if (!FileFilter(File)) return;
    //         var tfile = TagLib.File.Create(new RegularFileAbstraction((IRegularFile)File));
    //         var tag = tfile.Tag as TagLib.Image.CombinedImageTag;
    //
    //         var Tags = new Dictionary<string, object>();
    //
    //         Tags["Title"] = tag.Title;
    //         Tags["Creator"] = tag.Creator;
    //         Tags["Rating"] = tag.Rating;
    //         Tags["Keywords"] = tag.Keywords;
    //
    //         Tags["TagTypes"] = tag.TagTypes;
    //
    //         Tags["Image - Width"] = tfile.Properties.PhotoWidth;
    //         Tags["Image - Height"] = tfile.Properties.PhotoHeight;
    //         Tags["Image - Quality"] = tfile.Properties.PhotoQuality;
    //
    //         //if (tag.TagTypes == TagTypes.)
    //         Tags["Camera - Model"] = tag.Model;
    //         Tags["Camera - Make"] = tag.Make;
    //         Tags["Camera - FocalLengthIn35mmFilm"] = tag.FocalLengthIn35mmFilm;
    //         Tags["Camera - FocalLength"] = tag.FocalLength;
    //         Tags["Camera - ISOSpeedRatings"] = tag.ISOSpeedRatings;
    //         Tags["Camera - FNumber"] = tag.FNumber;
    //         Tags["Camera - ExposureTime"] = tag.ExposureTime;
    //         Tags["Camera - Software"] = tag.Software;
    //         Tags["Camera - Orientation"] = tag.Orientation;
    //         Tags["Camera - DateTime"] = tag.DateTime;
    //         Tags["Camera - TagTypes"] = tag.TagTypes;
    //
    //         Tags["GPS - Longitude"] = tag.Longitude;
    //         Tags["GPS - Latitude"] = tag.Latitude;
    //         Tags["GPS - Latitude"] = tag.Latitude;
    //         Tags["GPS - TagTypes"] = tag.TagTypes;
    //
    //     }
    //
    //
    //
    //     public class RegularFileAbstraction : TagLib.File.IFileAbstraction
    //     {
    //         public RegularFileAbstraction(IRegularFile File)
    //         {
    //             // The Taglib uses the extension to identify the file format,
    //             // we need to re-standardize the file extension in this class.
    //             int Index = Array.IndexOf(AllowMimeTypes, File.MimeType?.Mime);
    //
    //             string ExtensionName = MimeTypesExtensionName?[Index] ?? PathUtils.Extname(File.Name);
    //
    //             Name = PathUtils.Basename(File.Name) + ExtensionName;
    //
    //             ReadStream = File.Open();
    //             WriteStream = null;
    //         }
    //
    //         public void CloseStream(Stream stream)
    //         {
    //             stream.Dispose();
    //         }
    //
    //         public string Name { get; private set; }
    //
    //         public Stream ReadStream { get; private set; }
    //
    //         public Stream WriteStream { get; private set; }
    //     }
    // }
}