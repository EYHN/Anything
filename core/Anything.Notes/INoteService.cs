using System.Threading.Tasks;
using Anything.FileSystem;

namespace Anything.Notes
{
    public interface INoteService
    {
        public ValueTask<string> GetNote(FileHandle fileHandle);

        public ValueTask SetNote(FileHandle fileHandle, string node);
    }
}
