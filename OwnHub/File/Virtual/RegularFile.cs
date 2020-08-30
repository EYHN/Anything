using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using OwnHub.File.Base;

namespace OwnHub.File.Virtual
{
    public class RegularFile : BaseRegularFile
    {
        byte[] data;
        string pathName;

        public override string Path => pathName;

        public override string Name => PathUtils.Basename(pathName);

        public override Task<IFileStats> Stats => Task.FromResult<IFileStats>(null);

        public RegularFile(string pathName, byte[] data)
        {
            this.pathName = pathName;
            this.data = data;
        }

        public override Stream Open()
        {
            return new MemoryStream(data);
        }
    }
}
