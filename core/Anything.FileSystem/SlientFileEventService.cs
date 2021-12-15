using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.Disposables;

namespace Anything.FileSystem;

public class NullFileEventService : IFileEventService
{
    public ValueTask Emit(IEnumerable<FileEvent> fileEvents)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<IAsyncDisposable> Subscribe(Func<IEnumerable<FileEvent>, ValueTask> cb)
    {
#pragma warning disable CA2000
        return ValueTask.FromResult(AsyncDisposable.Create(() => ValueTask.CompletedTask) as IAsyncDisposable);
#pragma warning restore CA2000
    }

    public ValueTask WaitComplete()
    {
        return ValueTask.CompletedTask;
    }
}
