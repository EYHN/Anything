using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OwnHub.File;
using OwnHub.Preview.Icons;
using OwnHub.Preview.Metadata.Readers;
using OwnHub.Utils;

namespace OwnHub.Preview.Metadata
{
    public class MetadataService
    {
        private ILogger<MetadataService> logger;
        private MetadataCacheDatabase MetadataCacheDatabase { get; }
        
        private static readonly IMetadataReader[] MetadataReaders =
        {
            new ImageMetadataReader(),
            new ImagePaletteReader(),
            new FileInformationMetadataReader(),
        };
        
        public MetadataService(string databaseFile, ILogger<MetadataService> logger)
        {
            MetadataCacheDatabase = new MetadataCacheDatabase(databaseFile);
            MetadataCacheDatabase.Open().Wait();
            this.logger = logger;
        }

        private IEnumerable<IMetadataReader> MatchReaders(IFile file)
        {
            return MetadataReaders.Where(reader => reader.IsSupported(file)).ToArray();
        }

        public bool IsSupported(IFile file)
        {
            return MetadataReaders.Any(reader => reader.IsSupported(file));
        }

        public async Task<MetadataEntry> ReadMetadata(IFile file)
        {
            logger.LogDebug("Start reading - FileName:" + file.Name);
            // Get Cache Identifier
            string filePath = file.Path;

            // Get Cache Etag
            IFileStats? stats = await file.Stats;
            string? etag = stats?.ModifyTime != null && stats?.Size != null
                ? IconsDatabase.CalcFileEtag((DateTimeOffset) stats.ModifyTime, (long) stats.Size)
                : null;

            MetadataCache? metadataCache = null;
            try
            {
                metadataCache = await MetadataCacheDatabase.GetMetadata(filePath);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Read cache error!");
            }

            if (metadataCache != null)
            {
                if (metadataCache.Etag != etag)
                {
                    logger.LogDebug("Outdated cache - FileName:" + file.Name);
                }
                else
                {
                    logger.LogDebug("Cache hit - FileName:" + file.Name);
                    return metadataCache.Entry;
                }
            }
            
            MetadataEntry metadata = new MetadataEntry();
            IEnumerable<IMetadataReader> readers = MatchReaders(file);

            foreach (var reader in readers)
                try
                {
                    using Performance.Timing timing = Performance.StartTiming("Run MetadataReader - " + reader.Name, logger);
                    await reader.ReadMetadata(file, metadata);
                }
                catch (Exception err)
                {
                    logger.LogError(err, "Run MetadataReader Error - " + reader.Name + " -  FileName:" + file.Name);
                }

            if (etag != null)
            {
                await MetadataCacheDatabase.AddMetadata(filePath, etag, metadata);
            }

            return metadata;
        }
    }
}