using Anything.FileSystem;
using Anything.Preview.Metadata.Readers;
using Anything.Preview.MimeType;

namespace Anything.Preview.Metadata
{
    public class MetadataServiceFactory
    {
        public static IMetadataService BuildMetadataService(
            IFileService fileService,
            IMimeTypeService mimeTypeService)
        {
            var service = new MetadataService(fileService, mimeTypeService);
            service.RegisterRenderer(new FileInformationMetadataReader());
            service.RegisterRenderer(new ImageMetadataReader(fileService));
            return service;
        }
    }
}
