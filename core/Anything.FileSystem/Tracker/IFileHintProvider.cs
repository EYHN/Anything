using Anything.Utils.Event;

namespace Anything.FileSystem.Tracker
{
    public interface IFileHintProvider
    {
        public Event<FileHint> OnFileHint { get; }

        public Event<DirectoryHint> OnDirectoryHint { get; }

        public Event<DeletedHint> OnDeletedHint { get; }
    }
}
