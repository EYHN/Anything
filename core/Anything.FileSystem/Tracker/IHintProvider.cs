using Anything.Utils.Event;

namespace Anything.FileSystem.Tracker
{
    public interface IHintProvider
    {
        public Event<Hint> OnHint { get; }
    }
}
