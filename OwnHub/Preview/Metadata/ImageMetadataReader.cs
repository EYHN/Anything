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

namespace OwnHub.Preview.Metadata
{
    public class ImageMetadataReader
    {
        public static readonly string[] AllowMimeTypes = new string[]
        {
            "image/png", "image/jpeg", "image/bmp", "image/gif", "image/webp"
        };

        public bool FileFilter(IFile file)
        {
            if (file is IRegularFile)
            {
                if (AllowMimeTypes.Contains(file.MimeType.Mime))
                {
                    return true;
                }
            }
            return false;
        }

        public Dictionary<string, object> ReadImageMetadata(IFile File)
        {
            var Metadata = new Dictionary<string, object>();
            if (!FileFilter(File)) return Metadata;

            var RegularFile = File as IRegularFile;

            IReadOnlyList<MetadataExtractor.Directory> Directories;

            using (Stream ReadStream = RegularFile.Open())
            {
                if (File.MimeType.Mime == "image/png")
                    Directories = PngMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType.Mime == "image/jpeg")
                    Directories = JpegMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType.Mime == "image/bmp")
                    Directories = BmpMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType.Mime == "image/gif")
                    Directories = GifMetadataReader.ReadMetadata(ReadStream);
                else if (File.MimeType.Mime == "image/webp")
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

        public Dictionary<string, object> ParseJpegDirectory(JpegDirectory Directory, Dictionary<string, object> Metadata)
        {
            Metadata["Image - Width"] = Directory.GetImageWidth();
            Metadata["Image - Height"] = Directory.GetImageHeight();

            var NumberOfComponents = Directory.GetNumberOfComponents();
            Metadata["Image - Channels"] = NumberOfComponents;

            if (Directory.TryGetInt32(JpegDirectory.TagDataPrecision, out var DataPrecision))
                Metadata["Image - Bit Depth"] = DataPrecision * NumberOfComponents;

            if (Directory.ContainsTag(JpegDirectory.TagCompressionType))
                Metadata["[Advanced] Image - JPEG Compression Type"] = Directory.GetDescription(JpegDirectory.TagCompressionType);

            return Metadata;
        }

        public Dictionary<string, object> ParsePngDirectory(PngDirectory Directory, Dictionary<string, object> Metadata)
        {
            if (Directory.TryGetInt32(PngDirectory.TagImageWidth, out var Width))
                Metadata["Image - Width"] = Width;

            if (Directory.TryGetInt32(PngDirectory.TagImageHeight, out var Height))
                Metadata["Image - Height"] = Height;

            if (Directory.TryGetInt32(PngDirectory.TagColorType, out var ColorTypeID))
            {
                var ColorType = PngColorType.FromNumericValue(ColorTypeID);
                Metadata["Image - PNG Color Type"] = ColorType.Description;

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
                        Metadata["Image - Bit Depth"] = NumberOfComponents * DataPrecision;
                    else
                        Metadata["Image - Data Precision"] = DataPrecision;
            }

            if (Directory.TryGetDouble(PngDirectory.TagGamma, out var Gamma))
                Metadata["Image - Gamma"] = Gamma;

            if (Directory.ContainsTag(PngDirectory.TagCompressionType))
                Metadata["[Advanced] Image - Compression Type"] = Directory.GetDescription(PngDirectory.TagCompressionType);

            if (Directory.ContainsTag(PngDirectory.TagInterlaceMethod))
                Metadata["[Advanced] Image - Interlace Method"] = Directory.GetDescription(PngDirectory.TagInterlaceMethod);

            return Metadata;
        }

        public Dictionary<string, object> ParseExifDirectory(ExifDirectoryBase Directory, Dictionary<string, object> Metadata)
        {
            // https://www.exiv2.org/tags.html
            if (Directory.ContainsTag(ExifDirectoryBase.TagSubfileType))
                Metadata["Image - Subfile Type"] = Directory.GetDescription(ExifDirectoryBase.TagSubfileType);
            if (Directory.ContainsTag(ExifDirectoryBase.TagNewSubfileType))
                Metadata["Image - Subfile Type"] = Directory.GetDescription(ExifDirectoryBase.TagNewSubfileType);

            if (Directory.ContainsTag(ExifDirectoryBase.TagOrientation))
                Metadata["Image - Orientation"] = Directory.GetDescription(ExifDirectoryBase.TagOrientation);

            if (Directory.ContainsTag(ExifDirectoryBase.TagXResolution))
                Metadata["Image - X Resolution"] = Directory.GetDescription(ExifDirectoryBase.TagXResolution);

            if (Directory.ContainsTag(ExifDirectoryBase.TagYResolution))
                Metadata["Image - Y Resolution"] = Directory.GetDescription(ExifDirectoryBase.TagYResolution);

            if (Directory.TryGetDateTime(ExifDirectoryBase.TagDateTime, out DateTime DateTime))
                Metadata["Image - Date/Time"] = DateTime;

            if (Directory.ContainsTag(ExifDirectoryBase.TagColorSpace))
                Metadata["Image - Color Space"] = Directory.GetDescription(ExifDirectoryBase.TagColorSpace);

            if (Directory.ContainsTag(ExifDirectoryBase.TagUserComment))
                Metadata["Image - User Comment"] = Directory.GetDescription(ExifDirectoryBase.TagUserComment);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExifVersion))
                Metadata["Image - Exif Version"] = Directory.GetDescription(ExifDirectoryBase.TagExifVersion);

            if (Directory.TryGetInt32(ExifDirectoryBase.TagPageNumber, out var PageNumber))
                Metadata["Image - Page Number"] = PageNumber;


            if (Directory.ContainsTag(ExifDirectoryBase.TagMake))
                Metadata["Camera - Make"] = Directory.GetDescription(ExifDirectoryBase.TagMake);

            if (Directory.ContainsTag(ExifDirectoryBase.TagModel))
                Metadata["Camera - Model"] = Directory.GetDescription(ExifDirectoryBase.TagModel);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureTime))
                Metadata["Camera - Exposure Time"] = Directory.GetDescription(ExifDirectoryBase.TagExposureTime);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFNumber))
                Metadata["Camera - F-Number"] = Directory.GetDescription(ExifDirectoryBase.TagFNumber);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureProgram))
                Metadata["Camera - Exposure Program"] = Directory.GetDescription(ExifDirectoryBase.TagExposureProgram);

            if (Directory.ContainsTag(ExifDirectoryBase.TagShutterSpeed))
                Metadata["Camera - Shutter Speed"] = Directory.GetDescription(ExifDirectoryBase.TagShutterSpeed);

            if (Directory.ContainsTag(ExifDirectoryBase.TagIsoEquivalent))
                Metadata["Camera - ISO Speed"] = Directory.GetDescription(ExifDirectoryBase.TagIsoEquivalent);

            if (Directory.ContainsTag(ExifDirectoryBase.TagAperture))
                Metadata["Camera - Aperture"] = Directory.GetDescription(ExifDirectoryBase.TagAperture);

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureBias))
                Metadata["Camera - Exposure Bias"] = Directory.GetDescription(ExifDirectoryBase.TagExposureBias);

            if (Directory.ContainsTag(ExifDirectoryBase.TagMeteringMode))
                Metadata["Camera - Metering Mode"] = Directory.GetDescription(ExifDirectoryBase.TagMeteringMode);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFlash))
                Metadata["Camera - Flash"] = Directory.GetDescription(ExifDirectoryBase.TagFlash);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFocalLength))
                Metadata["Camera - Focal Length"] = Directory.GetDescription(ExifDirectoryBase.TagFocalLength);

            if (Directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime OriginalDateTime))
                Metadata["Camera - Date/Time Original"] = OriginalDateTime;

            if (Directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out DateTime DigitizedDateTime))
                Metadata["Camera - Date/Time Digitized"] = DigitizedDateTime;

            if (Directory.ContainsTag(ExifDirectoryBase.TagExposureMode))
                Metadata["Camera - Exposure Mode"] = Directory.GetDescription(ExifDirectoryBase.TagExposureMode);

            if (Directory.ContainsTag(ExifDirectoryBase.TagWhiteBalance))
                Metadata["Camera - White Balance"] = Directory.GetDescription(ExifDirectoryBase.TagWhiteBalance);

            if (Directory.ContainsTag(ExifDirectoryBase.TagWhiteBalanceMode))
                Metadata["Camera - White Balance Mode"] = Directory.GetDescription(ExifDirectoryBase.TagWhiteBalanceMode);

            if (Directory.ContainsTag(ExifDirectoryBase.TagSceneCaptureType))
                Metadata["Camera - Scene Capture Type"] = Directory.GetDescription(ExifDirectoryBase.TagSceneCaptureType);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensMake))
                Metadata["Camera - Lens Make"] = Directory.GetDescription(ExifDirectoryBase.TagLensMake);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensModel))
                Metadata["Camera - Lens Model"] = Directory.GetDescription(ExifDirectoryBase.TagLensModel);


            if (Directory.ContainsTag(ExifDirectoryBase.TagYCbCrPositioning))
                Metadata["[Advanced] Image - YCbCr Positioning"] = Directory.GetDescription(ExifDirectoryBase.TagYCbCrPositioning);

            if (Directory.ContainsTag(ExifDirectoryBase.TagComponentsConfiguration))
                Metadata["[Advanced] Image - Components Configuration"] = Directory.GetDescription(ExifDirectoryBase.TagComponentsConfiguration);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFocalPlaneXResolution))
                Metadata["[Advanced] Camera - Focal Plane X Resolution"] = Directory.GetDescription(ExifDirectoryBase.TagFocalPlaneXResolution);

            if (Directory.ContainsTag(ExifDirectoryBase.TagFocalPlaneYResolution))
                Metadata["[Advanced] Camera - Focal Plane Y Resolution"] = Directory.GetDescription(ExifDirectoryBase.TagFocalPlaneYResolution);

            if (Directory.ContainsTag(ExifDirectoryBase.TagCustomRendered))
                Metadata["[Advanced] Camera - Custom Rendered"] = Directory.GetDescription(ExifDirectoryBase.TagCustomRendered);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensSerialNumber))
                Metadata["[Advanced] Camera - Lens Serial Number"] = Directory.GetDescription(ExifDirectoryBase.TagLensSerialNumber);

            if (Directory.ContainsTag(ExifDirectoryBase.TagLensSpecification))
                Metadata["[Advanced] Camera - Lens Specification"] = Directory.GetDescription(ExifDirectoryBase.TagLensSpecification);

            if (Directory.ContainsTag(ExifDirectoryBase.TagInteropIndex))
                Metadata["[Advanced] Interoperability - Interoperability Index"] = Directory.GetDescription(ExifDirectoryBase.TagInteropIndex);

            if (Directory.ContainsTag(ExifDirectoryBase.TagInteropVersion))
                Metadata["[Advanced] Interoperability - Interoperability Version"] = Directory.GetDescription(ExifDirectoryBase.TagInteropVersion);


            return Metadata;
        }

        public Dictionary<string, object> ParseXmpDirectory(XmpDirectory Directory, Dictionary<string, object> Metadata)
        {
            var XmpProperties = Directory.GetXmpProperties();

            // aux https://www.exiv2.org/tags-xmp-aux.html
            if (XmpProperties.TryGetValue("aux:Lens", out var LensType))
                Metadata["Camera - Lens Model"] = LensType;

            if (XmpProperties.TryGetValue("aux:SerialNumber", out var LensSerialNumber))
                Metadata["[Advanced] Camera - Lens Serial Number"] = LensSerialNumber;


            // exifEX https://www.exiv2.org/tags-xmp-exifEX.html
            if (XmpProperties.TryGetValue("exifEX:LensMake", out var LensMakeExifEX))
                Metadata["Camera - Lens Make"] = LensMakeExifEX;

            if (XmpProperties.TryGetValue("exifEX:LensModel", out var LensTypeExifEX))
                Metadata["Camera - Lens Model"] = LensTypeExifEX;

            if (XmpProperties.TryGetValue("exifEX:LensSerialNumber", out var LensSerialNumberExifEX))
                Metadata["[Advanced] Camera - Lens Serial Number"] = LensSerialNumberExifEX;


            return Metadata;
        }

        public Dictionary<string, object> ParseCanonMakernoteDirectory(CanonMakernoteDirectory Directory, Dictionary<string, object> Metadata)
        {
            if (Directory.ContainsTag(CanonMakernoteDirectory.CameraSettings.TagLensType))
                Metadata["Camera - Lens Model"] = Directory.GetDescription(CanonMakernoteDirectory.CameraSettings.TagLensType);

            return Metadata;
        }
    }
}
