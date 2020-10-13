using OwnHub.File;
using MetadataExtractor;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using MetadataExtractor.Formats.Xmp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OwnHub.Test.Preview.Metadata;

namespace OwnHub.Preview.Metadata
{
    public class ImageMetadataReader: IMetadataReader
    {
        public static readonly string[] AllowMimeTypes = new string[]
        {
            "image/png", "image/jpeg", "image/bmp", "image/gif", "image/webp"
        };

        public bool IsSupported(IFile file)
        {
            if (file is IRegularFile)
            {
                if (AllowMimeTypes.Contains(file.MimeType?.Mime))
                {
                    return true;
                }
            }
            return false;
        }

        public MetadataEntry ReadImageMetadata(IFile File, MetadataEntry Metadata)
        {
            if (!IsSupported(File)) return Metadata;

            var RegularFile = File as IRegularFile;

            IReadOnlyList<MetadataExtractor.Directory> Directories;

            using (Stream ReadStream = RegularFile.Open())
            {
                if (File.MimeType?.Mime == "image/png")
                    Directories = PngMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType?.Mime == "image/jpeg")
                    Directories = JpegMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType?.Mime == "image/bmp")
                    Directories = BmpMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType?.Mime == "image/gif")
                    Directories = GifMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType?.Mime == "image/webp")
                    Directories = WebPMetadataReader.ReadMetadata(ReadStream);
                else
                    Directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(ReadStream);
            }
            

            foreach (var Directory in Directories)
            {
                if (Directory is JpegDirectory)
                {
                    ParseJpegDirectory(Directory as JpegDirectory, Metadata);
                }
                else if (Directory is ExifDirectoryBase)
                {
                    ParseExifDirectory(Directory as ExifDirectoryBase, Metadata);
                }
                else if (Directory is PngDirectory)
                {
                    ParsePngDirectory(Directory as PngDirectory, Metadata);
                }
                else if (Directory is XmpDirectory)
                {
                    ParseXmpDirectory(Directory as XmpDirectory, Metadata);
                }
                else if (Directory is CanonMakernoteDirectory)
                {
                    ParseCanonMakernoteDirectory(Directory as CanonMakernoteDirectory, Metadata);
                }
            }

            return Metadata;

        }

        public MetadataEntry ParseJpegDirectory(JpegDirectory Directory, MetadataEntry Metadata)
        {
            Metadata.Image.Width = Directory.GetImageWidth();
            Metadata.Image.Height = Directory.GetImageHeight();

            var NumberOfComponents = Directory.GetNumberOfComponents();
            Metadata.Image.Channels = NumberOfComponents;

            if (Directory.TryGetInt32(JpegDirectory.TagDataPrecision, out var DataPrecision))
                Metadata.Image.BitDepth = DataPrecision * NumberOfComponents;

            if (Directory.ContainsTag(JpegDirectory.TagCompressionType))
                Metadata.Image.JPEGCompressionType = Directory.GetDescription(JpegDirectory.TagCompressionType);

            return Metadata;
        }

        public MetadataEntry ParsePngDirectory(PngDirectory Directory, MetadataEntry Metadata)
        {
            if (Directory.TryGetInt32(PngDirectory.TagImageWidth, out var Width))
                Metadata.Image.Width = Width;

            if (Directory.TryGetInt32(PngDirectory.TagImageHeight, out var Height))
                Metadata.Image.Height = Height;

            if (Directory.TryGetInt32(PngDirectory.TagColorType, out var ColorTypeID))
            {
                var ColorType = PngColorType.FromNumericValue(ColorTypeID);
                Metadata.Image.PNGColorType = ColorType.Description;

                int? NumberOfComponents = null;

                if (ColorType == PngColorType.Greyscale) NumberOfComponents = 1;
                else if (ColorType == PngColorType.TrueColor) NumberOfComponents = 3;
                else if (ColorType == PngColorType.GreyscaleWithAlpha) NumberOfComponents = 2;
                else if (ColorType == PngColorType.TrueColorWithAlpha) NumberOfComponents = 4;

                int? DataPrecision = null;
                if (Directory.TryGetInt32(PngDirectory.TagBitsPerSample, out var BitsPerSample))
                    DataPrecision = BitsPerSample;

                if (DataPrecision != null)
                    if (NumberOfComponents != null)
                        Metadata.Image.BitDepth = (int)(NumberOfComponents * DataPrecision);
                    else
                        Metadata.Image.DataPrecision = (int)DataPrecision;
            }

            if (Directory.TryGetDouble(PngDirectory.TagGamma, out var Gamma))
                Metadata.Image.Gamma = Gamma;

            if (Directory.ContainsTag(PngDirectory.TagCompressionType))
                Metadata.Image.CompressionType = Directory.GetDescription(PngDirectory.TagCompressionType);

            if (Directory.ContainsTag(PngDirectory.TagInterlaceMethod))
                Metadata.Image.InterlaceMethod = Directory.GetDescription(PngDirectory.TagInterlaceMethod);

            return Metadata;
        }

        public MetadataEntry ParseExifDirectory(ExifDirectoryBase Directory, MetadataEntry Metadata)
        {
            // https://www.exiv2.org/tags.html
            if (Directory.ContainsTag(ExifDirectoryBase.TagSubfileType))
                Metadata.Image.SubfileType = Directory.GetDescription(ExifDirectoryBase.TagSubfileType);
            if (Directory.ContainsTag(ExifDirectoryBase.TagNewSubfileType))
                Metadata.Image.SubfileType = Directory.GetDescription(ExifDirectoryBase.TagNewSubfileType);

            if (Directory.ContainsTag(ExifDirectoryBase.TagOrientation))
                Metadata.Image.Orientation = Directory.GetDescription(ExifDirectoryBase.TagOrientation);

            if (Directory.ContainsTag(ExifDirectoryBase.TagXResolution))
                Metadata.Image.XResolution = Directory.GetDescription(ExifDirectoryBase.TagXResolution);

            if (Directory.ContainsTag(ExifDirectoryBase.TagYResolution))
                Metadata.Image.YResolution = Directory.GetDescription(ExifDirectoryBase.TagYResolution);

            if (Directory.TryGetDateTime(ExifDirectoryBase.TagDateTime, out DateTime DateTime))
                Metadata.Image.DateTime = DateTime;

            if (Directory.ContainsTag(ExifDirectoryBase.TagColorSpace))
                Metadata.Image.ColorSpace = Directory.GetDescription(ExifDirectoryBase.TagColorSpace);

            if (Directory.ContainsTag(ExifDirectoryBase.TagUserComment))
                Metadata.Image.UserComment = Directory.GetDescription(ExifDirectoryBase.TagUserComment);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExifVersion))
                Metadata.Image.ExifVersion = Directory.GetDescription(ExifDirectoryBase.TagExifVersion);

            if (Directory.TryGetInt32(ExifDirectoryBase.TagPageNumber, out var PageNumber))
                Metadata.Image.PageNumber = PageNumber;


            if (Directory.ContainsTag(ExifDirectoryBase.TagMake))
                Metadata.Camera.Make = Directory.GetDescription(ExifDirectoryBase.TagMake);

            if (Directory.ContainsTag(ExifDirectoryBase.TagModel))
                Metadata.Camera.Model = Directory.GetDescription(ExifDirectoryBase.TagModel);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureTime))
                Metadata.Camera.ExposureTime = Directory.GetDescription(ExifDirectoryBase.TagExposureTime);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFNumber))
                Metadata.Camera.FNumber = Directory.GetDescription(ExifDirectoryBase.TagFNumber);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureProgram))
                Metadata.Camera.ExposureProgram = Directory.GetDescription(ExifDirectoryBase.TagExposureProgram);

            if (Directory.ContainsTag(ExifDirectoryBase.TagShutterSpeed))
                Metadata.Camera.ShutterSpeed = Directory.GetDescription(ExifDirectoryBase.TagShutterSpeed);

            if (Directory.ContainsTag(ExifDirectoryBase.TagIsoEquivalent))
                Metadata.Camera.ISOSpeed = Directory.GetDescription(ExifDirectoryBase.TagIsoEquivalent);

            if (Directory.ContainsTag(ExifDirectoryBase.TagAperture))
                Metadata.Camera.Aperture = Directory.GetDescription(ExifDirectoryBase.TagAperture);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureBias))
                Metadata.Camera.ExposureBias = Directory.GetDescription(ExifDirectoryBase.TagExposureBias);

            if (Directory.ContainsTag(ExifDirectoryBase.TagMeteringMode))
                Metadata.Camera.MeteringMode = Directory.GetDescription(ExifDirectoryBase.TagMeteringMode);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFlash))
                Metadata.Camera.Flash = Directory.GetDescription(ExifDirectoryBase.TagFlash);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFocalLength))
                Metadata.Camera.FocalLength = Directory.GetDescription(ExifDirectoryBase.TagFocalLength);

            if (Directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime OriginalDateTime))
                Metadata.Camera.DateTimeOriginal = OriginalDateTime;

            if (Directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out DateTime DigitizedDateTime))
                Metadata.Camera.DateTimeDigitized = DigitizedDateTime;

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureMode))
                Metadata.Camera.ExposureMode = Directory.GetDescription(ExifDirectoryBase.TagExposureMode);

            if (Directory.ContainsTag(ExifDirectoryBase.TagWhiteBalance))
                Metadata.Camera.WhiteBalance = Directory.GetDescription(ExifDirectoryBase.TagWhiteBalance);

            if (Directory.ContainsTag(ExifDirectoryBase.TagWhiteBalanceMode))
                Metadata.Camera.WhiteBalanceMode = Directory.GetDescription(ExifDirectoryBase.TagWhiteBalanceMode);

            if (Directory.ContainsTag(ExifDirectoryBase.TagSceneCaptureType))
                Metadata.Camera.SceneCaptureType = Directory.GetDescription(ExifDirectoryBase.TagSceneCaptureType);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensMake))
                Metadata.Camera.LensMake = Directory.GetDescription(ExifDirectoryBase.TagLensMake);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensModel))
                Metadata.Camera.LensModel = Directory.GetDescription(ExifDirectoryBase.TagLensModel);


            if (Directory.ContainsTag(ExifDirectoryBase.TagYCbCrPositioning))
                Metadata.Image.YCbCrPositioning = Directory.GetDescription(ExifDirectoryBase.TagYCbCrPositioning);

            if (Directory.ContainsTag(ExifDirectoryBase.TagComponentsConfiguration))
                Metadata.Image.ComponentsConfiguration = Directory.GetDescription(ExifDirectoryBase.TagComponentsConfiguration);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFocalPlaneXResolution))
                Metadata.Camera.FocalPlaneXResolution = Directory.GetDescription(ExifDirectoryBase.TagFocalPlaneXResolution);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFocalPlaneYResolution))
                Metadata.Camera.FocalPlaneYResolution = Directory.GetDescription(ExifDirectoryBase.TagFocalPlaneYResolution);

            if (Directory.ContainsTag(ExifDirectoryBase.TagCustomRendered))
                Metadata.Camera.CustomRendered = Directory.GetDescription(ExifDirectoryBase.TagCustomRendered);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensSerialNumber))
                Metadata.Camera.LensSerialNumber = Directory.GetDescription(ExifDirectoryBase.TagLensSerialNumber);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensSpecification))
                Metadata.Camera.LensSpecification = Directory.GetDescription(ExifDirectoryBase.TagLensSpecification);

            if (Directory.ContainsTag(ExifDirectoryBase.TagInteropIndex))
                Metadata.Interoperability.InteroperabilityIndex = Directory.GetDescription(ExifDirectoryBase.TagInteropIndex);

            if (Directory.ContainsTag(ExifDirectoryBase.TagInteropVersion))
                Metadata.Interoperability.InteroperabilityVersion = Directory.GetDescription(ExifDirectoryBase.TagInteropVersion);

            return Metadata;
        }

        public MetadataEntry ParseXmpDirectory(XmpDirectory Directory, MetadataEntry Metadata)
        {
            var XmpProperties = Directory.GetXmpProperties();

            // aux https://www.exiv2.org/tags-xmp-aux.html
            if (XmpProperties.TryGetValue("aux:Lens", out var LensType))
                Metadata.Camera.LensModel = LensType;

            if (XmpProperties.TryGetValue("aux:SerialNumber", out var LensSerialNumber))
                Metadata.Camera.LensSerialNumber = LensSerialNumber;


            // exifEX https://www.exiv2.org/tags-xmp-exifEX.html
            if (XmpProperties.TryGetValue("exifEX:LensMake", out var LensMakeExifEX))
                Metadata.Camera.LensMake = LensMakeExifEX;

            if (XmpProperties.TryGetValue("exifEX:LensModel", out var LensTypeExifEX))
                Metadata.Camera.LensModel = LensTypeExifEX;

            if (XmpProperties.TryGetValue("exifEX:LensSerialNumber", out var LensSerialNumberExifEX))
                Metadata.Camera.LensSerialNumber = LensSerialNumberExifEX;


            return Metadata;
        }

        public MetadataEntry ParseCanonMakernoteDirectory(CanonMakernoteDirectory Directory, MetadataEntry Metadata)
        {
            if (Directory.ContainsTag(CanonMakernoteDirectory.CameraSettings.TagLensType))
                Metadata.Camera.LensModel = Directory.GetDescription(CanonMakernoteDirectory.CameraSettings.TagLensType);

            return Metadata;
        }
    }
}
