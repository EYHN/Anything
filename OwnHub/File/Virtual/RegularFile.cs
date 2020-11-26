using System.IO;
using System.Threading.Tasks;
using OwnHub.File.Base;

namespace OwnHub.File.Virtual
{
    public class RegularFile : BaseRegularFile
    {
        private readonly byte[] data;
        private readonly string pathName;

        public RegularFile(string pathName, byte[] data)
        {
            this.pathName = pathName;
            this.data = data;
        }

        public override string Path => pathName;

        public override string Name => PathUtils.Basename(pathName);

        public override Task<IFileStats?> Stats => Task.FromResult<IFileStats?>(null);

        public override Stream Open()
        {
            return new MemoryStream(data);
        }
    }
}