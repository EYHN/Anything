using OwnHub.File.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File.Local
{
    public class RegularFile : BaseRegularFile
    {
        FileInfo FileInfo;

        string PathName;

        public override string Path => PathName;

        public override string Name => PathUtils.Basename(PathName);

        public override Task<IFileStats> Stats => Task.FromResult((IFileStats)new FileStats(this.FileInfo));

        public RegularFile(string PathName, FileInfo FileInfo)
        {
            this.FileInfo = FileInfo;
            this.PathName = PathName;
        }

        public string GetRealPath()
        {
            return this.FileInfo.FullName;
        }

        public override Stream Open()
        {
            return this.FileInfo.Open(FileMode.Open, FileAccess.Read);
        }
    }
}
