using OwnHub.File;
using OwnHub.Test.Preview.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Metadata
{
    public class MetadataService
    {
        public static IMetadataReader[] MetadataReaders = new IMetadataReader[] {
            new ImageMetadataReader()
        };

        public IMetadataReader[] MatchReaders(IFile file)
        {
            return MetadataReaders.Where((Reader) =>
            {
                return Reader.IsSupported(file);
            }).ToArray();
        }

        public bool IsSupported(IFile file)
        {
            return MetadataReaders.Any((Reader) => Reader.IsSupported(file));
        }

        public MetadataEntry ReadImageMetadata(IFile File)
        {
            MetadataEntry Metadata = new MetadataEntry();
            IMetadataReader[] Readers = MatchReaders(File);

            foreach (var Reader in Readers)
            {
                try
                {
                    Reader.ReadImageMetadata(File, Metadata);
                } catch (Exception err)
                {
                    Debug.WriteLine(err.ToString());
                }
            }

            return Metadata;
        }
    }
}
