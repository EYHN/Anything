using System;

namespace Anything.Preview.Metadata.Schema
{
    public class CameraMetadata : IMetadata
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

        [MetadataAdvanced]
        public string? FocalPlaneXResolution { get; set; }

        [MetadataAdvanced]
        public string? FocalPlaneYResolution { get; set; }

        [MetadataAdvanced]
        public string? CustomRendered { get; set; }

        [MetadataAdvanced]
        public string? LensSerialNumber { get; set; }

        [MetadataAdvanced]
        public string? LensSpecification { get; set; }
    }
}
