using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anything.FileSystem;

public interface IFileEventHandler
{
    public ValueTask OnFileEvent(IEnumerable<FileEvent> events);
}
