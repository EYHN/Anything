using System.Text;
using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Notes;

public class NoteService : INoteService
{
    private readonly IFileService _fileService;

    public NoteService(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async ValueTask<string> GetNotes(FileHandle fileHandle)
    {
        var noteBuffer = await _fileService.GetProperty(fileHandle, "note");
        return noteBuffer == null ? "" : Encoding.UTF8.GetString(noteBuffer.Value.Span);
    }

    public async ValueTask SetNotes(FileHandle fileHandle, string notes)
    {
        await _fileService.SetProperty(fileHandle, "note", Encoding.UTF8.GetBytes(notes));
    }
}
