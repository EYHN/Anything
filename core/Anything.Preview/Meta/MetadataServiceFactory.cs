using Anything.FileSystem;
using Anything.Preview.Meta.Readers;
using Anything.Preview.Mime;

namespace Anything.Preview.Meta
{
    public static class MetadataServiceFactory
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
