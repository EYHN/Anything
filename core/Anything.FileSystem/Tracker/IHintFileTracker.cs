using System.Threading.Tasks;

namespace Anything.FileSystem.Tracker
{
    public interface IHintFileTracker : IFileTracker
    {
        public ValueTask CommitHint(Hint hint);

        /// <summary>
        ///     Test only. Wait for all pending tasks to be completed.
        /// </summary>
        public ValueTask WaitComplete();
    }
}
