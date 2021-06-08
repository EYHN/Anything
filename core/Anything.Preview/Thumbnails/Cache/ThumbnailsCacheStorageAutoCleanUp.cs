using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
using Anything.Utils;

namespace Anything.Preview.Thumbnails.Cache
{
    public static class ThumbnailsCacheStorageAutoCleanUp
    {
        private const string MetadataKey = "Thumbnails_Auto_Clean_Up";

        public static void RegisterAutoCleanUp(ThumbnailsCacheDatabaseStorage storage, IFileSystemService fileSystem)
        {
            storage.OnBeforeCache.On(
                async url =>
                {
                    await fileSystem.AttachTag(url, new FileTrackTag(MetadataKey), true);
                });

            fileSystem.OnFileChange +=
                events =>
                {
                    var deleteList = new List<Url>();
                    foreach (var @event in events)
                    {
                        if (@event.Type is FileChangeEvent.EventType.Changed or FileChangeEvent.EventType.Deleted &&
                            @event.Tags.Any(metadata => metadata.Key == MetadataKey))
                        {
                            deleteList.Add(@event.Url);
                        }
                    }

                    Task.Run(() => storage.DeleteBatch(deleteList.ToArray()));
                };
        }
    }
}
