using System;

namespace Anything.Preview.Metadata.Schema
{
    public class InformationMetadata : IMetadata
    {
        public DateTimeOffset? CreationTime { get; set; }

        public DateTimeOffset? LastWriteTime { get; set; }
    }
}
