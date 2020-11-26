using System.IO;

namespace OwnHub.File.Base
{
    public abstract class BaseRegularFile : BaseFile, IRegularFile
    {
        public override MimeType? MimeType => MimeTypeRules.DefaultRules.Match(PathUtils.Extname(Path));

        public abstract Stream Open();
    }
}