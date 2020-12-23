using System;
using System.Collections.Generic;
using System.Reflection;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OwnHub.Preview.Metadata
{
    public interface IMetadataEntry
    {
    }

    public class MetadataEntry : IMetadataEntry
    {
        public MetadataEntry()
        {
            Information = new InformationMetadataEntry();
            Image = new ImageMetadataEntry();
            Camera = new CameraMetadataEntry();
            Interoperability = new InteroperabilityMetadataEntry();
        }

        public InformationMetadataEntry Information { get; set; }

        public string? Palette { get; set; }

        public ImageMetadataEntry Image { get; set; }

        public CameraMetadataEntry Camera { get; set; }

        [MetadataAdvanced] public InteroperabilityMetadataEntry Interoperability { get; set; }

        private static List<string> ToMetadataNamesList(string? parent, bool parentAdvanced,
            List<string> outList, Type metadataEntryType)
        {
            Type type = metadataEntryType;
            foreach (PropertyInfo property in type.GetProperties())
            {
                string name = property.Name;
                bool advanced = parentAdvanced || (property.GetCustomAttribute<MetadataAdvanced>()?.Advanced ?? false);
                
                if (typeof(IMetadataEntry).IsAssignableFrom(property.PropertyType))
                {
                    ToMetadataNamesList(parent != null ? parent + "." + name : name, advanced, outList, property.PropertyType);
                }
                else
                {
                    outList.Add((advanced ? "[Advanced] " : "") + (parent != null ? parent + "." : "") + name);
                }
            }

            return outList;
        }

        public static List<string> ToMetadataNamesList()
        {
            return ToMetadataNamesList(null, parentAdvanced: false, new List<string>(), typeof(MetadataEntry));
        }

        private static Dictionary<string, object> ToDictionary(string? parent, bool parentAdvanced,
            Dictionary<string, object> outDictionary, IMetadataEntry entry)
        {
            Type type = entry.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                string name = property.Name;
                object? value = property.GetValue(entry);
                bool advanced = parentAdvanced || (property.GetCustomAttribute<MetadataAdvanced>()?.Advanced ?? false);

                if (value == null) continue;

                if (value is IMetadataEntry metadataEntry)
                {
                    ToDictionary(parent != null ? parent + "." + name : name, advanced, outDictionary, metadataEntry);
                }
                else if (value is string || value is DateTimeOffset || value.GetType().IsPrimitive)
                {
                    outDictionary[
                        (advanced ? "[Advanced] " : "") + (parent != null ? parent + "." : "") + name
                    ] = value;
                }
            }

            return outDictionary;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return ToDictionary(null, parentAdvanced: false, new Dictionary<string, object>(), this);
        }
    }

    public class InformationMetadataEntry : IMetadataEntry
    {
        public DateTimeOffset? CreationTime { get; set; }
        
        public DateTimeOffset? ModifyTime { get; set; }
    }

    public class ImageMetadataEntry : IMetadataEntry
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Channels { get; set; }

        public int? BitDepth { get; set; }

        public int? DataPrecision { get; set; }

        public double? Gamma { get; set; }

        public string? SubfileType { get; set; }

        public string? Orientation { get; set; }

        public string? XResolution { get; set; }

        public string? YResolution { get; set; }

        public DateTimeOffset? DateTime { get; set; }

        public string? ColorSpace { get; set; }

        public string? UserComment { get; set; }

        public string? ExifVersion { get; set; }

        public int? PageNumber { get; set; }
        
        [MetadataAdvanced] public string? PngColorType { get; set; }

        [MetadataAdvanced] public string? CompressionType { get; set; }

        [MetadataAdvanced] public string? InterlaceMethod { get; set; }

        [MetadataAdvanced] public string? YCbCrPositioning { get; set; }

        [MetadataAdvanced] public string? ComponentsConfiguration { get; set; }
        
        /// <summary>
        /// 0 = Baseline
        /// 1 = Extended sequential, Huffman
        /// 2 = Progressive, Huffman
        /// 3 = Lossless, Huffman
        /// 4 = Unknown
        /// 5 = Differential sequential, Huffman
        /// 6 = Differential progressive, Huffman
        /// 7 = Differential lossless, Huffman
        /// 8 = Reserved for JPEG extensions
        /// 9 = Extended sequential, arithmetic
        /// 10 = Progressive, arithmetic
        /// 11 = Lossless, arithmetic
        /// 12 = Unknown
        /// 13 = Differential sequential, arithmetic
        /// 14 = Differential progressive, arithmetic
        /// 15 = Differential lossless, arithmetic
        /// </summary>
        [MetadataAdvanced]
        public int? JpegCompressionType { get; set; }
    }

    public class CameraMetadataEntry : IMetadataEntry
    {
        public string? Make { get; set; }

        public string? Model { get; set; }

        public string? ExposureTime { get; set; }

        public string? FNumber { get; set; }

        public string? ExposureProgram { get; set; }

        public string? ShutterSpeed { get; set; }

        public string? IsoSpeed { get; set; }

        public string? Aperture { get; set; }

        public string? ExposureBias { get; set; }

        public string? MeteringMode { get; set; }

        public string? Flash { get; set; }

        public string? FocalLength { get; set; }

        public DateTimeOffset? DateTimeOriginal { get; set; }

        public DateTimeOffset? DateTimeDigitized { get; set; }

        public string? ExposureMode { get; set; }

        public string? WhiteBalance { get; set; }

        public string? WhiteBalanceMode { get; set; }

        public string? SceneCaptureType { get; set; }

        public string? LensMake { get; set; }

        public string? LensModel { get; set; }

        [MetadataAdvanced] public string? FocalPlaneXResolution { get; set; }

        [MetadataAdvanced] public string? FocalPlaneYResolution { get; set; }

        [MetadataAdvanced] public string? CustomRendered { get; set; }

        [MetadataAdvanced] public string? LensSerialNumber { get; set; }

        [MetadataAdvanced] public string? LensSpecification { get; set; }
    }

    public class InteroperabilityMetadataEntry : IMetadataEntry
    {
        [MetadataAdvanced] public string? InteroperabilityIndex { get; set; }

        [MetadataAdvanced] public string? InteroperabilityVersion { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataAdvanced : Attribute
    {
        public MetadataAdvanced(bool advanced = true)
        {
            this.Advanced = advanced;
        }

        public bool Advanced { get; }
    }
}