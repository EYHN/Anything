using OwnHub.FileSystem;
using OwnHub.Preview.MimeType;
using OwnHub.Preview.Thumbnails.Cache;
using OwnHub.Utils;

namespace OwnHub.Preview.Thumbnails
{
    public class ThumbnailsService : IThumbnailsService
    {
        private readonly IMimeTypeService _mimeType;

        private readonly IThumbnailsCacheStorage _thumbnailsCache;

        private readonly IFileSystemService _fileSystem;

        public ThumbnailsService(IFileSystemService fileSystem, IMimeTypeService mimeType, IThumbnailsCacheStorage thumbnailsCache)
        {
            _fileSystem = fileSystem;
            _mimeType = mimeType;
            _thumbnailsCache = thumbnailsCache;
        }

        public IThumbnails GetThumbnail(Url url, ThumbnailOption option)
        {
            throw new System.NotImplementedException();
        }
    }
}
