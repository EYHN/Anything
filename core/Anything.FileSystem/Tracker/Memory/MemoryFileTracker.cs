using System.Threading;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Database.Provider;
using Anything.FileSystem.Tracker.Database;
using Anything.Utils;
using Anything.Utils.Event;

namespace Anything.FileSystem.Tracker.Memory
{
    public class MemoryFileTracker : IFileTracker
    {
        private static int _sequenceId;

        private readonly DatabaseFileTracker _databaseFileTracker;

        public MemoryFileTracker(IFileHintProvider fileHintProvider)
        {
            var connectionProvider = new SharedMemoryConnectionProvider("memory-file-tracker-" + Interlocked.Increment(ref _sequenceId));
            _databaseFileTracker = new DatabaseFileTracker(fileHintProvider, new SqliteContext(connectionProvider));
        }

        public ValueTask AttachTag(Url url, FileTrackTag trackTag, bool replace = false)
        {
            return _databaseFileTracker.AttachTag(url, trackTag, replace);
        }

        public ValueTask<FileTrackTag[]> GetTags(Url url)
        {
            return _databaseFileTracker.GetTags(url);
        }

        public Event<FileChangeEvent[]> OnFileChange => _databaseFileTracker.OnFileChange;

        public ValueTask Create()
        {
            return _databaseFileTracker.Create();
        }
    }
}
