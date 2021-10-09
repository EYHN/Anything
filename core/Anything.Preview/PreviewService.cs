using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Icons;
using Anything.Preview.Meta;
using Anything.Preview.Meta.Schema;
using Anything.Preview.Mime;
using Anything.Preview.Mime.Schema;
using Anything.Preview.Thumbnails;
using Anything.Utils;

namespace Anything.Preview
{
    public class PreviewService : IPreviewService
    {
        private readonly IIconsService _iconsService;

        private readonly IMetadataService _metadataService;

        private readonly IMimeTypeService _mimeTypeService;

        private readonly IThumbnailsService _thumbnailsService;

        public PreviewService(
            IFileService fileService,
            MimeTypeRules mimeTypeRules,
            IPreviewCacheStorage cacheStorage)
        {
            _iconsService = new IconsService(fileService);
            _mimeTypeService = new MimeTypeService(fileService, mimeTypeRules);
            _thumbnailsService = ThumbnailsServiceFactory.BuildThumbnailsService(
                fileService,
                _mimeTypeService,
                cacheStorage.ThumbnailsCacheStorage);
            _metadataService = MetadataServiceFactory.BuildMetadataService(fileService, _mimeTypeService);
        }

        public ValueTask<bool> IsSupportThumbnail(FileHandle fileHandle)
        {
            return _thumbnailsService.IsSupportThumbnail(fileHandle);
        }

        public ValueTask<IThumbnail?> GetThumbnail(FileHandle fileHandle, ThumbnailOption option)
        {
            return _thumbnailsService.GetThumbnail(fileHandle, option);
        }

        public ValueTask<string> GetIconId(FileHandle fileHandle)
        {
            return _iconsService.GetIconId(fileHandle);
        }

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option)
        {
            return _iconsService.GetIconImage(id, option);
        }

        public ValueTask<MimeType?> GetMimeType(FileHandle fileHandle, MimeTypeOption option)
        {
            return _mimeTypeService.GetMimeType(fileHandle, option);
        }

        public ValueTask<Metadata> GetMetadata(FileHandle fileHandle)
        {
            return _metadataService.ReadMetadata(fileHandle);
        }
    }
}
