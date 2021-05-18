using System.IO;
using System.Text;
using System.Threading.Tasks;
using StagingBox.File.Base;
using StagingBox.Utils;

namespace StagingBox.File.Virtual
{
    public class RegularFile : BaseRegularFile
    {
        public readonly byte[] Data = null!;
        public readonly string PathName = null!;

        public RegularFile()
        {
        }

        public RegularFile(string pathName)
        {
            PathName = pathName;
            Data = new byte[0];
        }

        public RegularFile(string pathName, string data)
        {
            PathName = pathName;
            Data = Encoding.UTF8.GetBytes(data);
        }

        public RegularFile(string pathName, byte[] data)
        {
            PathName = pathName;
            Data = data;
        }

        public override string Path => PathName;

        public override string Name => PathLib.Basename(PathName);

        public override Task<IFileStats?> Stats => Task.FromResult<IFileStats?>(null);

        public override Stream Open()
        {
            return new MemoryStream(Data);
        }
    }
}
