using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview.Mime.Schema;

namespace Anything.Preview.Mime;

public interface IMimeTypeService
{
    public ValueTask<MimeType?> GetMimeType(FileHandle fileHandle);
}
