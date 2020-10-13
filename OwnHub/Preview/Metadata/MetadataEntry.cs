using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OwnHub.Test.Preview.Metadata
{
    public interface IMetadataEntry {}

    [JsonConverter(typeof(MetadataEntryConverter))]
    public class MetadataEntry: IMetadataEntry
    {
        [MetadataName("Image")]
        public ImageMetadataEntry Image { get; set; }

        [MetadataName("Camera")]
        public CameraMetadataEntry Camera { get; set; }

        [MetadataAdvanced]
        [MetadataName("Interoperability")]
        public InteroperabilityMetadataEntry Interoperability { get; set; }

        public MetadataEntry()
        {
            Image = new ImageMetadataEntry();
            Camera = new CameraMetadataEntry();
            Interoperability = new InteroperabilityMetadataEntry();
        }
    }
    
    public class ImageMetadataEntry: IMetadataEntry
    {
        [MetadataName("Width")]
        public int? Width { get; set; }

        [MetadataName("Height")]
        public int? Height { get; set; }

        [MetadataName("Channels")]
        public int? Channels { get; set; }

        [MetadataName("Bit Depth")]
        public int? BitDepth { get; set; }

        [MetadataName("PNG Color Type")]
        public string PNGColorType { get; set; }

        [MetadataName("Data Precision")]
        public int? DataPrecision { get; set; }

        [MetadataName("Gamma")]
        public double? Gamma { get; set; }

        [MetadataName("Subfile Type")]
        public string SubfileType { get; set; }

        [MetadataName("Orientation")]
        public string Orientation { get; set; }

        [MetadataName("X Resolution")]
        public string XResolution { get; set; }

        [MetadataName("Y Resolution")]
        public string YResolution { get; set; }

        [MetadataName("Date/Time")]
        public DateTime? DateTime { get; set; }

        [MetadataName("Color Space")]
        public string ColorSpace { get; set; }

        [MetadataName("User Comment")]
        public string UserComment { get; set; }

        [MetadataName("Exif Version")]
        public string ExifVersion { get; set; }

        [MetadataName("Page Number")]
        public int? PageNumber { get; set; }

        [MetadataAdvanced]
        [MetadataName("Compression Type")]
        public string CompressionType { get; set; }

        [MetadataAdvanced]
        [MetadataName("Interlace Method")]
        public string InterlaceMethod { get; set; }

        [MetadataAdvanced]
        [MetadataName("YCbCr Positioning")]
        public string YCbCrPositioning { get; set; }

        [MetadataAdvanced]
        [MetadataName("Components Configuration")]
        public string ComponentsConfiguration { get; set; }

        [MetadataAdvanced]
        [MetadataName("JPEG Compression Type")]
        public string JPEGCompressionType { get; set; }
    }

    public class CameraMetadataEntry: IMetadataEntry
    {
        [MetadataName("Make")]
        public string Make { get; set; }

        [MetadataName("Model")]
        public string Model { get; set; }

        [MetadataName("Exposure Time")]
        public string ExposureTime { get; set; }

        [MetadataName("F-Number")]
        public string FNumber { get; set; }

        [MetadataName("Exposure Program")]
        public string ExposureProgram { get; set; }

        [MetadataName("Shutter Speed")]
        public string ShutterSpeed { get; set; }

        [MetadataName("ISO Speed")]
        public string ISOSpeed { get; set; }

        [MetadataName("Aperture")]
        public string Aperture { get; set; }

        [MetadataName("Exposure Bias")]
        public string ExposureBias { get; set; }

        [MetadataName("Metering Mode")]
        public string MeteringMode { get; set; }

        [MetadataName("Flash")]
        public string Flash { get; set; }

        [MetadataName("Focal Length")]
        public string FocalLength { get; set; }

        [MetadataName("Date/Time Original")]
        public DateTime? DateTimeOriginal { get; set; }

        [MetadataName("Date/Time Digitized")]
        public DateTime? DateTimeDigitized { get; set; }

        [MetadataName("Exposure Mode")]
        public string ExposureMode { get; set; }

        [MetadataName("White Balance")]
        public string WhiteBalance { get; set; }

        [MetadataName("White Balance Mode")]
        public string WhiteBalanceMode { get; set; }

        [MetadataName("Scene Capture Type")]
        public string SceneCaptureType { get; set; }

        [MetadataName("Lens Make")]
        public string LensMake { get; set; }

        [MetadataName("Lens Model")]
        public string LensModel { get; set; }

        [MetadataAdvanced]
        [MetadataName("Focal Plane X Resolution")]
        public string FocalPlaneXResolution { get; set; }

        [MetadataAdvanced]
        [MetadataName("Focal Plane Y Resolution")]
        public string FocalPlaneYResolution { get; set; }

        [MetadataAdvanced]
        [MetadataName("Custom Rendered")]
        public string CustomRendered { get; set; }

        [MetadataAdvanced]
        [MetadataName("Lens Serial Number")]
        public string LensSerialNumber { get; set; }

        [MetadataAdvanced]
        [MetadataName("Lens Specification")]
        public string LensSpecification { get; set; }
    }

    public class InteroperabilityMetadataEntry: IMetadataEntry
    {
        [MetadataAdvanced]
        [MetadataName("Interoperability Index")]
        public string InteroperabilityIndex { get; set; }

        [MetadataAdvanced]
        [MetadataName("Interoperability Version")]
        public string InteroperabilityVersion { get; set; }
    }

    public class MetadataEntryConverter : JsonConverter<MetadataEntry>
    {
        public override MetadataEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public void WriteMetadataEntry(string Parent, bool ParentAdvanced, Utf8JsonWriter Writer, object Entry, JsonSerializerOptions Options)
        {
            Type type = Entry.GetType();
            foreach (PropertyInfo Property in type.GetProperties())
            {
                string Name = Property.GetCustomAttribute<MetadataName>()?.Name ?? Property.Name;
                object Value = Property.GetValue(Entry);
                bool Advanced = ParentAdvanced || (Property.GetCustomAttribute<MetadataAdvanced>()?.Advanced ?? false);

                if (Value == null) continue;

                if (Value is IMetadataEntry)
                {
                    WriteMetadataEntry(Parent != null ? Parent + " " + Name : Name, Advanced, Writer, Value, Options);
                } else
                {
                    Writer.WritePropertyName((Advanced ? "[Advanced] " : "") + (Parent != null ? Parent + " - " : "") + Name);
                    JsonSerializer.Serialize(Writer, Value, Options);
                }
            }
        }

        public override void Write(Utf8JsonWriter Writer, MetadataEntry Entry, JsonSerializerOptions Options)
        {
            Writer.WriteStartObject();
            WriteMetadataEntry(null, false, Writer, Entry, Options);
            Writer.WriteEndObject();
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataName : System.Attribute
    {
        public string Name { get; }
        public MetadataName(string Name)
        {
            this.Name = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class MetadataAdvanced : System.Attribute
    {
        public bool Advanced { get; }
        public MetadataAdvanced(bool Advanced = true)
        {
            this.Advanced = Advanced;
        }
    }
}
