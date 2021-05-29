namespace Anything.Preview.Metadata.Schema
{
    public class InteroperabilityMetadata : IMetadata
    {
        [MetadataAdvanced]
        public string? InteroperabilityIndex { get; set; }

        [MetadataAdvanced]
        public string? InteroperabilityVersion { get; set; }
    }
}
