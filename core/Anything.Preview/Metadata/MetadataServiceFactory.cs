using Anything.FileSystem;
using Anything.Preview.Metadata.Readers;
using Anything.Preview.MimeType;

namespace Anything.Preview.Metadata
{
    public class MetadataServiceFactory
    {
        public static IMetadataService BuildMetadataService(
            IFileSystemService fileSystemService,
            IMimeTypeService mimeTypeService)
        {
            var service = new MetadataService(fileSystemService, mimeTypeService);
            service.RegisterRenderer(new FileInformationMetadataReader());
            service.RegisterRenderer(new ImageMetadataReader(fileSystemService));
            return service;
        }
    }
}
