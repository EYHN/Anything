using System.IO;
using StagingBox.Utils;

namespace StagingBox.File.Base
{
    public abstract class BaseRegularFile : BaseFile, IRegularFile
    {
        public override MimeType? MimeType => MimeTypeRules.DefaultRules.Match(PathLib.Extname(Path));

        public abstract Stream Open();
    }
}
