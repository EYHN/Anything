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
    public class PreviewService : Disposable, IPreviewService
    {
        private readonly IIconsService _iconsService;

        private readonly IMetadataService _metadataService;

        private readonly IMimeTypeService _mimeTypeService;

        private readonly ThumbnailsService _thumbnailsService;

        public PreviewService(
            IFileService fileService,
            MimeTypeRules mimeTypeRules,
            IPreviewCacheStorage cacheStorage)
        {
            _iconsService = new IconsService(fileService);
            _mimeTypeService = new MimeTypeService(mimeTypeRules);
            _thumbnailsService = ThumbnailsServiceFactory.BuildThumbnailsService(
                fileService,
                _mimeTypeService,
                cacheStorage.ThumbnailsCacheStorage);
            _metadataService = MetadataServiceFactory.BuildMetadataService(fileService, _mimeTypeService);
        }

        public ValueTask<bool> IsSupportThumbnail(Url url)
        {
            return _thumbnailsService.IsSupportThumbnail(url);
        }

        public ValueTask<IThumbnail?> GetThumbnail(Url url, ThumbnailOption option)
        {
            return _thumbnailsService.GetThumbnail(url, option);
        }

        public ValueTask<string> GetIconId(Url url)
        {
            return _iconsService.GetIconId(url);
        }

        public ValueTask<IIconImage> GetIconImage(string id, IconImageOption option)
        {
            return _iconsService.GetIconImage(id, option);
        }

        public ValueTask<MimeType?> GetMimeType(Url url, MimeTypeOption option)
        {
            return _mimeTypeService.GetMimeType(url, option);
        }

        public ValueTask<Metadata> GetMetadata(Url url)
        {
            return _metadataService.ReadMetadata(url);
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _thumbnailsService.Dispose();
        }
    }
}
