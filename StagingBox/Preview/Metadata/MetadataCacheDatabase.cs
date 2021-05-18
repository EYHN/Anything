using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using StagingBox.File.Fork;

namespace StagingBox.Preview.Metadata
{
    public class MetadataCacheDatabase: FileForkDatabase
    {
        public MetadataCacheDatabase(string databaseFile) : base(databaseFile)
        {

        }

        public async Task<MetadataCache> AddMetadata(string parentFile, string etag, MetadataEntry entry)
        {
            MetadataCache metadataCache = new MetadataCache(parentFile, new MetadataCachePayload(etag, entry));
            await Add(metadataCache, "Metadata" + ":" + parentFile);
            return metadataCache;
        }

        public async Task<MetadataCache?> GetMetadata(string parentFile)
        {
            return await GetFork<MetadataCache>(parentFile);
        }
    }

    public class MetadataCachePayload {
        public string Etag { get; }
        public MetadataEntry Entry { get; }

        public MetadataCachePayload(string etag, MetadataEntry entry)
        {
            Etag = etag;
            Entry = entry;
        }
    }

    public sealed class MetadataCache : FileFork<MetadataCachePayload>
    {
        public string Etag => Payload.Etag;
        public MetadataEntry Entry => Payload.Entry;
        public override MetadataCachePayload Payload { get; set; }

        public MetadataCache(string parentFile) : base(parentFile)
        {
            Payload = null!;
        }

        public MetadataCache(string parentFile, MetadataCachePayload payload) : base(parentFile)
        {
            Payload = payload;
        }
    }
}
