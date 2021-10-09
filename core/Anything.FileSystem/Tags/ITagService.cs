using System.Threading.Tasks;
using Anything.Utils;

namespace Anything.FileSystem.Tags
{
    public interface ITagService
    {
        public ValueTask<Tag[]> GetTags(Url url);

        public ValueTask SetTags(Url url, Tag[] tags);
    }
}
