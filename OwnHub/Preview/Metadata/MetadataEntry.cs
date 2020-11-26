using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OwnHub.Test.Preview.Metadata
{
    public interface IMetadataEntry
    {
    }
    
    public class MetadataEntry : IMetadataEntry
    {
        public MetadataEntry()
        {
            Image = new ImageMetadataEntry();
            Camera = new CameraMetadataEntry();
            Interoperability = new InteroperabilityMetadataEntry();
        }

        [MetadataName("Image")] public ImageMetadataEntry Image { get; set; }

        [MetadataName("Camera")] public CameraMetadataEntry Camera { get; set; }

        [MetadataAdvanced]
        [MetadataName("Interoperability")]
        public InteroperabilityMetadataEntry Interoperability { get; set; }
        
        public static Dictionary<string, object> ToDictionary(string? parent, bool parentAdvanced, Dictionary<string, object> outDictionary, IMetadataEntry entry)
        {
            Type type = entry.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                string name = property.GetCustomAttribute<MetadataName>()?.Name ?? property.Name;
                object? value = property.GetValue(entry);
                bool advanced = parentAdvanced || (property.GetCustomAttribute<MetadataAdvanced>()?.Advanced ?? false);

                if (value == null) continue;

                if (value is IMetadataEntry)
                {
                    ToDictionary(parent != null ? parent + " " + name : name, advanced, outDictionary, (IMetadataEntry) value);
                }
                else
                {
                    outDictionary[
                            (advanced ? "[Advanced] " : "") + (parent != null ? parent + " - " : "") + name
                        ] =value;
                }
            }

            return outDictionary;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return ToDictionary(null, parentAdvanced: false, new Dictionary<string, object>(), this);
        }
    }

    public class ImageMetadataEntry : IMetadataEntry
    {
        [MetadataName("Width")] public int? Width { get; set; }

        [MetadataName("Height")] public int? Height { get; set; }

        [MetadataName("Channels")] public int? Channels { get; set; }

        [MetadataName("Bit Depth")] public int? BitDepth { get; set; }

        [MetadataName("PNG Color Type")] public string? PngColorType { get; set; }

        [MetadataName("Data Precision")] public int? DataPrecision { get; set; }

        [MetadataName("Gamma")] public double? Gamma { get; set; }

        [MetadataName("Subfile Type")] public string? SubfileType { get; set; }

        [MetadataName("Orientation")] public string? Orientation { get; set; }

        [MetadataName("X Resolution")] public string? XResolution { get; set; }

        [MetadataName("Y Resolution")] public string? YResolution { get; set; }

        [MetadataName("Date/Time")] public DateTime? DateTime { get; set; }

        [MetadataName("Color Space")] public string? ColorSpace { get; set; }

        [MetadataName("User Comment")] public string? UserComment { get; set; }

        [MetadataName("Exif Version")] public string? ExifVersion { get; set; }

        [MetadataName("Page Number")] public int? PageNumber { get; set; }

        [MetadataAdvanced]
        [MetadataName("Compression Type")]
        public string? CompressionType { get; set; }

        [MetadataAdvanced]
        [MetadataName("Interlace Method")]
        public string? InterlaceMethod { get; set; }

        [MetadataAdvanced]
        [MetadataName("YCbCr Positioning")]
        public string? YCbCrPositioning { get; set; }

        [MetadataAdvanced]
        [MetadataName("Components Configuration")]
        public string? ComponentsConfiguration { get; set; }

        [MetadataAdvanced]
        [MetadataName("JPEG Compression Type")]
        public string? JpegCompressionType { get; set; }
    }

    public class CameraMetadataEntry : IMetadataEntry
    {
        [MetadataName("Make")] public string? Make { get; set; }

        [MetadataName("Model")] public string? Model { get; set; }

        [MetadataName("Exposure Time")] public string? ExposureTime { get; set; }

        [MetadataName("F-Number")] public string? FNumber { get; set; }

        [MetadataName("Exposure Program")] public string? ExposureProgram { get; set; }

        [MetadataName("Shutter Speed")] public string? ShutterSpeed { get; set; }

        [MetadataName("ISO Speed")] public string? IsoSpeed { get; set; }

        [MetadataName("Aperture")] public string? Aperture { get; set; }

        [MetadataName("Exposure Bias")] public string? ExposureBias { get; set; }

        [MetadataName("Metering Mode")] public string? MeteringMode { get; set; }

        [MetadataName("Flash")] public string? Flash { get; set; }

        [MetadataName("Focal Length")] public string? FocalLength { get; set; }

        [MetadataName("Date/Time Original")] public DateTime? DateTimeOriginal { get; set; }

        [MetadataName("Date/Time Digitized")] public DateTime? DateTimeDigitized { get; set; }

        [MetadataName("Exposure Mode")] public string? ExposureMode { get; set; }

        [MetadataName("White Balance")] public string? WhiteBalance { get; set; }

        [MetadataName("White Balance Mode")] public string? WhiteBalanceMode { get; set; }

        [MetadataName("Scene Capture Type")] public string? SceneCaptureType { get; set; }

        [MetadataName("Lens Make")] public string? LensMake { get; set; }

        [MetadataName("Lens Model")] public string? LensModel { get; set; }

        [MetadataAdvanced]
        [MetadataName("Focal Plane X Resolution")]
        public string? FocalPlaneXResolution { get; set; }

        [MetadataAdvanced]
        [MetadataName("Focal Plane Y Resolution")]
        public string? FocalPlaneYResolution { get; set; }

        [MetadataAdvanced]
        [MetadataName("Custom Rendered")]
        public string? CustomRendered { get; set; }

        [MetadataAdvanced]
        [MetadataName("Lens Serial Number")]
        public string? LensSerialNumber { get; set; }

        [MetadataAdvanced]
        [MetadataName("Lens Specification")]
        public string? LensSpecification { get; set; }
    }

    public class InteroperabilityMetadataEntry : IMetadataEntry
    {
        [MetadataAdvanced]
        [MetadataName("Interoperability Index")]
        public string? InteroperabilityIndex { get; set; }

        [MetadataAdvanced]
        [MetadataName("Interoperability Version")]
        public string? InteroperabilityVersion { get; set; }
    }

    public class MetadataEntryConverter : JsonConverter<MetadataEntry>
    {
        public override MetadataEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new InvalidOperationException();
        }

        public void WriteMetadataEntry(string? parent, bool parentAdvanced, Utf8JsonWriter writer, object entry,
            JsonSerializerOptions options)
        {
            Type type = entry.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                string name = property.GetCustomAttribute<MetadataName>()?.Name ?? property.Name;
                object? value = property.GetValue(entry);
                bool advanced = parentAdvanced || (property.GetCustomAttribute<MetadataAdvanced>()?.Advanced ?? false);

                if (value == null) continue;

                if (value is IMetadataEntry)
                {
                    WriteMetadataEntry(parent != null ? parent + " " + name : name, advanced, writer, value, options);
                }
                else
                {
                    writer.WritePropertyName((advanced ? "[Advanced] " : "") + (parent != null ? parent + " - " : "") +
                                             name);
                    JsonSerializer.Serialize(writer, value, options);
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, MetadataEntry entry, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteMetadataEntry(null, false, writer, entry, options);
            writer.WriteEndObject();
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataName : Attribute
    {
        public MetadataName(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
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