using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Notes;

public interface INoteService
{
    public ValueTask<string> GetNotes(FileHandle fileHandle);

    public ValueTask SetNotes(FileHandle fileHandle, string notes);
}
