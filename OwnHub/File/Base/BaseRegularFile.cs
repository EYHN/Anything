using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File.Base
{
    public abstract class BaseRegularFile : BaseFile, IRegularFile
    {
        public override MimeType? MimeType => MimeTypeRules.DefaultRules.Match(PathUtils.Extname(Path));

        public abstract Stream Open();
    }
}
