using System.Collections.Generic;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Utils;
using MetadataExtractor;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using MetadataExtractor.Formats.Xmp;
using Directory = MetadataExtractor.Directory;

namespace Anything.Preview.Metadata.Readers
{
    public class ImageMetadataReader : BaseMetadataReader
    {
        public string Name { get; } = "ImageMetadataReader";

        protected override string[] SupportMimeTypes => new[] { "image/png", "image/jpeg", "image/bmp", "image/gif", "image/webp" };

        private readonly IFileSystemService _fileSystem;

        public ImageMetadataReader(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool IsSupported(Url url)
        {
            return false;
        }

        protected override async Task<Schema.Metadata> ReadMetadata(
            Schema.Metadata metadata,
            MetadataReaderFileInfo fileInfo,
            MetadataReaderOption option)
        {
            IReadOnlyList<Directory> directories;

            using (var readStream = await _fileSystem.OpenReadFileStream(fileInfo.Url))
            {
                directories = fileInfo.MimeType switch
                {
                    "image/png" => PngMetadataReader.ReadMetadata(readStream),
                    "image/jpeg" => JpegMetadataReader.ReadMetadata(readStream),
                    "image/bmp" => BmpMetadataReader.ReadMetadata(readStream),
                    "image/gif" => GifMetadataReader.ReadMetadata(readStream),
                    "image/webp" => WebPMetadataReader.ReadMetadata(readStream),
                    _ => MetadataExtractor.ImageMetadataReader.ReadMetadata(readStream)
                };
            }

            foreach (var directory in directories)
            {
                if (directory is JpegDirectory jpegDirectory)
                {
                    ParseJpegDirectory(jpegDirectory, metadata);
                }
                else if (directory is WebPDirectory webPDirectory)
                {
                    ParseWebPDirectory(webPDirectory, metadata);
                }
                else if (directory is ExifIfd0Directory exifIfd0Directory)
                {
                    ParseExifDirectory(exifIfd0Directory, metadata);
                }
                else if (directory is ExifSubIfdDirectory exifSubIfdDirectory)
                {
                    ParseExifDirectory(exifSubIfdDirectory, metadata);
                }
                else if (directory is PngDirectory pngDirectory)
                {
                    ParsePngDirectory(pngDirectory, metadata);
                }
                else if (directory is XmpDirectory xmpDirectory)
                {
                    ParseXmpDirectory(xmpDirectory, metadata);
                }
                else if (directory is CanonMakernoteDirectory canonMakernoteDirectory)
                {
                    ParseCanonMakernoteDirectory(canonMakernoteDirectory, metadata);
                }
            }

            return metadata;
        }

        public Schema.Metadata ParseJpegDirectory(JpegDirectory directory, Schema.Metadata metadata)
        {
            metadata.Image.Width = directory.GetImageWidth();
            metadata.Image.Height = directory.GetImageHeight();

            var numberOfComponents = directory.GetNumberOfComponents();
            metadata.Image.Channels = numberOfComponents;

            if (directory.TryGetInt32(JpegDirectory.TagDataPrecision, out var dataPrecision))
            {
                metadata.Image.BitDepth = dataPrecision * numberOfComponents;
            }

            if (directory.TryGetInt32(JpegDirectory.TagCompressionType, out var jpegCompressionType))
            {
                metadata.Image.JpegCompressionType = jpegCompressionType;
            }

            return metadata;
        }

        public Schema.Metadata ParsePngDirectory(PngDirectory directory, Schema.Metadata metadata)
        {
            if (directory.TryGetInt32(PngDirectory.TagImageWidth, out var width))
            {
                metadata.Image.Width = width;
            }

            if (directory.TryGetInt32(PngDirectory.TagImageHeight, out var height))
            {
                metadata.Image.Height = height;
            }

            if (directory.TryGetInt32(PngDirectory.TagColorType, out var colorTypeId))
            {
                var colorType = PngColorType.FromNumericValue(colorTypeId);
                metadata.Image.PngColorType = colorType.Description;

                int? numberOfComponents = null;

                if (colorType == PngColorType.Greyscale)
                {
                    numberOfComponents = 1;
                }
                else if (colorType == PngColorType.TrueColor)
                {
                    numberOfComponents = 3;
                }
                else if (colorType == PngColorType.GreyscaleWithAlpha)
                {
                    numberOfComponents = 2;
                }
                else if (colorType == PngColorType.TrueColorWithAlpha)
                {
                    numberOfComponents = 4;
                }

                int? dataPrecision = null;
                if (directory.TryGetInt32(PngDirectory.TagBitsPerSample, out var bitsPerSample))
                {
                    dataPrecision = bitsPerSample;
                }

                if (dataPrecision != null)
                {
                    if (numberOfComponents != null)
                    {
                        metadata.Image.BitDepth = (int)(numberOfComponents * dataPrecision);
                    }
                    else
                    {
                        metadata.Image.DataPrecision = (int)dataPrecision;
                    }
                }
            }

            if (directory.TryGetDouble(PngDirectory.TagGamma, out var gamma))
            {
                metadata.Image.Gamma = gamma;
            }

            if (directory.ContainsTag(PngDirectory.TagCompressionType))
            {
                metadata.Image.CompressionType = directory.GetDescription(PngDirectory.TagCompressionType);
            }

            if (directory.ContainsTag(PngDirectory.TagInterlaceMethod))
            {
                metadata.Image.InterlaceMethod = directory.GetDescription(PngDirectory.TagInterlaceMethod);
            }

            return metadata;
        }

        public Schema.Metadata ParseWebPDirectory(WebPDirectory directory, Schema.Metadata metadata)
        {
            if (directory.TryGetInt32(WebPDirectory.TagImageWidth, out var width))
            {
                metadata.Image.Width = width;
            }

            if (directory.TryGetInt32(WebPDirectory.TagImageHeight, out var height))
            {
                metadata.Image.Height = height;
            }

            return metadata;
        }

        public Schema.Metadata ParseExifDirectory(ExifDirectoryBase directory, Schema.Metadata metadata)
        {
            // https://www.exiv2.org/tags.html
            if (directory.ContainsTag(ExifDirectoryBase.TagSubfileType))
            {
                metadata.Image.SubfileType = directory.GetDescription(ExifDirectoryBase.TagSubfileType);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagNewSubfileType))
            {
                metadata.Image.SubfileType = directory.GetDescription(ExifDirectoryBase.TagNewSubfileType);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagOrientation))
            {
                metadata.Image.Orientation = directory.GetDescription(ExifDirectoryBase.TagOrientation);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagXResolution))
            {
                metadata.Image.XResolution = directory.GetDescription(ExifDirectoryBase.TagXResolution);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagYResolution))
            {
                metadata.Image.YResolution = directory.GetDescription(ExifDirectoryBase.TagYResolution);
            }

            if (directory.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dateTime))
            {
                metadata.Image.DateTime = dateTime;
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagColorSpace))
            {
                metadata.Image.ColorSpace = directory.GetDescription(ExifDirectoryBase.TagColorSpace);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagUserComment))
            {
                metadata.Image.UserComment = directory.GetDescription(ExifDirectoryBase.TagUserComment);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagExifVersion))
            {
                metadata.Image.ExifVersion = directory.GetDescription(ExifDirectoryBase.TagExifVersion);
            }

            if (directory.TryGetInt32(ExifDirectoryBase.TagPageNumber, out var pageNumber))
            {
                metadata.Image.PageNumber = pageNumber;
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagMake))
            {
                metadata.Camera.Make = directory.GetDescription(ExifDirectoryBase.TagMake);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagModel))
            {
                metadata.Camera.Model = directory.GetDescription(ExifDirectoryBase.TagModel);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagExposureTime))
            {
                metadata.Camera.ExposureTime = directory.GetDescription(ExifDirectoryBase.TagExposureTime);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagFNumber))
            {
                metadata.Camera.FNumber = directory.GetDescription(ExifDirectoryBase.TagFNumber);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagExposureProgram))
            {
                metadata.Camera.ExposureProgram = directory.GetDescription(ExifDirectoryBase.TagExposureProgram);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagShutterSpeed))
            {
                metadata.Camera.ShutterSpeed = directory.GetDescription(ExifDirectoryBase.TagShutterSpeed);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagIsoEquivalent))
            {
                metadata.Camera.IsoSpeed = directory.GetDescription(ExifDirectoryBase.TagIsoEquivalent);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagAperture))
            {
                metadata.Camera.Aperture = directory.GetDescription(ExifDirectoryBase.TagAperture);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagExposureBias))
            {
                metadata.Camera.ExposureBias = directory.GetDescription(ExifDirectoryBase.TagExposureBias);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagMeteringMode))
            {
                metadata.Camera.MeteringMode = directory.GetDescription(ExifDirectoryBase.TagMeteringMode);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagFlash))
            {
                metadata.Camera.Flash = directory.GetDescription(ExifDirectoryBase.TagFlash);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagFocalLength))
            {
                metadata.Camera.FocalLength = directory.GetDescription(ExifDirectoryBase.TagFocalLength);
            }

            if (directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var originalDateTime))
            {
                metadata.Camera.DateTimeOriginal = originalDateTime;
            }

            if (directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var digitizedDateTime))
            {
                metadata.Camera.DateTimeDigitized = digitizedDateTime;
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagExposureMode))
            {
                metadata.Camera.ExposureMode = directory.GetDescription(ExifDirectoryBase.TagExposureMode);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagWhiteBalance))
            {
                metadata.Camera.WhiteBalance = directory.GetDescription(ExifDirectoryBase.TagWhiteBalance);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagWhiteBalanceMode))
            {
                metadata.Camera.WhiteBalanceMode = directory.GetDescription(ExifDirectoryBase.TagWhiteBalanceMode);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagSceneCaptureType))
            {
                metadata.Camera.SceneCaptureType = directory.GetDescription(ExifDirectoryBase.TagSceneCaptureType);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagLensMake))
            {
                metadata.Camera.LensMake = directory.GetDescription(ExifDirectoryBase.TagLensMake);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagLensModel))
            {
                metadata.Camera.LensModel = directory.GetDescription(ExifDirectoryBase.TagLensModel);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagYCbCrPositioning))
            {
                metadata.Image.YCbCrPositioning = directory.GetDescription(ExifDirectoryBase.TagYCbCrPositioning);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagComponentsConfiguration))
            {
                metadata.Image.ComponentsConfiguration =
                    directory.GetDescription(ExifDirectoryBase.TagComponentsConfiguration);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagFocalPlaneXResolution))
            {
                metadata.Camera.FocalPlaneXResolution =
                    directory.GetDescription(ExifDirectoryBase.TagFocalPlaneXResolution);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagFocalPlaneYResolution))
            {
                metadata.Camera.FocalPlaneYResolution =
                    directory.GetDescription(ExifDirectoryBase.TagFocalPlaneYResolution);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagCustomRendered))
            {
                metadata.Camera.CustomRendered = directory.GetDescription(ExifDirectoryBase.TagCustomRendered);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagLensSerialNumber))
            {
                metadata.Camera.LensSerialNumber = directory.GetDescription(ExifDirectoryBase.TagLensSerialNumber);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagLensSpecification))
            {
                metadata.Camera.LensSpecification = directory.GetDescription(ExifDirectoryBase.TagLensSpecification);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagInteropIndex))
            {
                metadata.Interoperability.InteroperabilityIndex =
                    directory.GetDescription(ExifDirectoryBase.TagInteropIndex);
            }

            if (directory.ContainsTag(ExifDirectoryBase.TagInteropVersion))
            {
                metadata.Interoperability.InteroperabilityVersion =
                    directory.GetDescription(ExifDirectoryBase.TagInteropVersion);
            }

            return metadata;
        }

        public Schema.Metadata ParseXmpDirectory(XmpDirectory directory, Schema.Metadata metadata)
        {
            var xmpProperties = directory.GetXmpProperties();

            // aux https://www.exiv2.org/tags-xmp-aux.html
            if (xmpProperties.TryGetValue("aux:Lens", out var lensType))
            {
                metadata.Camera.LensModel = lensType;
            }

            if (xmpProperties.TryGetValue("aux:SerialNumber", out var lensSerialNumber))
            {
                metadata.Camera.LensSerialNumber = lensSerialNumber;
            }

            // exifEX https://www.exiv2.org/tags-xmp-exifEX.html
            if (xmpProperties.TryGetValue("exifEX:LensMake", out var lensMakeExifEx))
            {
                metadata.Camera.LensMake = lensMakeExifEx;
            }

            if (xmpProperties.TryGetValue("exifEX:LensModel", out var lensTypeExifEx))
            {
                metadata.Camera.LensModel = lensTypeExifEx;
            }

            if (xmpProperties.TryGetValue("exifEX:LensSerialNumber", out var lensSerialNumberExifEx))
            {
                metadata.Camera.LensSerialNumber = lensSerialNumberExifEx;
            }

            return metadata;
        }

        public Schema.Metadata ParseCanonMakernoteDirectory(CanonMakernoteDirectory directory, Schema.Metadata metadata)
        {
            if (directory.ContainsTag(CanonMakernoteDirectory.CameraSettings.TagLensType))
            {
                metadata.Camera.LensModel =
                    directory.GetDescription(CanonMakernoteDirectory.CameraSettings.TagLensType);
            }

            return metadata;
        }
    }
}
