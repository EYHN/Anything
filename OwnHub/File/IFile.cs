using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public interface IFile
    {
        public string Path { get; }

        public string Name { get; }

        public MimeType? MimeType { get; }

        public Task<IFileStats> Stats { get; }
    }
}
