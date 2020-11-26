using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.File;
using OwnHub.Preview.Icons;
using OwnHub.Test.Preview.Metadata;
using OwnHub.Utils;

namespace OwnHub.Preview.Metadata
{
    public class MetadataService
    {
        private MetadataCacheDatabase MetadataCacheDatabase { get; }
        
        public static IMetadataReader[] MetadataReaders =
        {
            new ImageMetadataReader()
        };
        
        public MetadataService(SqliteConnectionFactory connectionFactory)
        {
            MetadataCacheDatabase = new MetadataCacheDatabase(connectionFactory.Make(SqliteOpenMode.ReadWriteCreate));
            MetadataCacheDatabase.Open().Wait();
        }

        public IMetadataReader[] MatchReaders(IFile file)
        {
            return MetadataReaders.Where(reader => { return reader.IsSupported(file); }).ToArray();
        }

        public bool IsSupported(IFile file)
        {
            return MetadataReaders.Any(reader => reader.IsSupported(file));
        }

        public async Task<MetadataEntry> ReadMetadata(IFile file)
        {
            // Get Cache Identifier
            string filePath = file.Path;

            // Clac Cache Etag
            IFileStats? stats = await file.Stats;
            string? etag = stats != null && stats.ModifyTime != null && stats.Size != null
                ? IconsDatabase.CalcFileEtag((DateTimeOffset) stats.ModifyTime, (long) stats.Size)
                : null;

            MetadataCache? metadataCache = await MetadataCacheDatabase.GetMetadata(filePath);

            if (metadataCache != null)
            {
                if (metadataCache.Etag != etag)
                {
                    await metadataCache.Delete();
                }
                else
                {
                    return metadataCache.Entry;
                }
            }
            
            MetadataEntry metadata = new MetadataEntry();
            IMetadataReader[] readers = MatchReaders(file);

            foreach (var reader in readers)
                try
                {
                    await reader.ReadMetadata(file, metadata);
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err.ToString());
                }

            if (etag != null)
            {
                await MetadataCacheDatabase.AddMetadata(filePath, etag, metadata);
            }

            return metadata;
        }
    }
}