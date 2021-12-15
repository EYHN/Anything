using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Preview.Thumbnails;

public interface IThumbnailsService
{
    public ValueTask<bool> IsSupportThumbnail(FileHandle fileHandle);

    public ValueTask<ThumbnailImage?> GetThumbnailImage(FileHandle fileHandle, ThumbnailOption option);
}
