using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anything.FileSystem;

public interface IFileEventService
{
    public ValueTask Emit(IEnumerable<FileEvent> fileEvents);

    public ValueTask Emit(FileEvent fileEvent)
    {
        return Emit(new[] { fileEvent });
    }

    public ValueTask<IAsyncDisposable> Subscribe(Func<IEnumerable<FileEvent>, ValueTask> cb);

    /// <summary>
    ///     Test only. Wait for all pending tasks to be completed.
    /// </summary>
    public ValueTask WaitComplete();
}
