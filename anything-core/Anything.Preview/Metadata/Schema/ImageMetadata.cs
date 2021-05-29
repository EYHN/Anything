using System;

namespace Anything.Preview.Metadata.Schema
{
    public class ImageMetadata : IMetadata
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

        [MetadataAdvanced]
        public string? PngColorType { get; set; }

        [MetadataAdvanced]
        public string? CompressionType { get; set; }

        [MetadataAdvanced]
        public string? InterlaceMethod { get; set; }

        [MetadataAdvanced]
        public string? YCbCrPositioning { get; set; }

        [MetadataAdvanced]
        public string? ComponentsConfiguration { get; set; }

        /// <summary>
        /// Gets or sets JpegCompressionType.
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
        /// 15 = Differential lossless, arithmetic.
        /// </summary>
        [MetadataAdvanced]
        public int? JpegCompressionType { get; set; }
    }
}
