using OwnHub.File;
using OwnHub.Test.Preview.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Metadata
{
    public interface IMetadataReader
    {
        public bool IsSupported(IFile file);

        public MetadataEntry ReadImageMetadata(IFile File, MetadataEntry Metadata);
    }
}
