using OwnHub.File.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File.Local
{
    public class FileStats: BaseFileStats
    {
        bool isDirectory;
        FileInfo file;
        DirectoryInfo directory;

        FileSystemInfo fileSystemInfo => this.isDirectory ? (FileSystemInfo)directory : (FileSystemInfo)file;

        public override long? Size => this.isDirectory ? null : (long?)file.Length;

        public override DateTimeOffset? ModifyTime => new DateTimeOffset(fileSystemInfo.LastWriteTimeUtc);

        public override DateTimeOffset? AccessTime => new DateTimeOffset(fileSystemInfo.LastAccessTimeUtc);

        public override DateTimeOffset? CreationTime => new DateTimeOffset(fileSystemInfo.CreationTimeUtc);

        public FileStats(FileInfo file)
        {
            this.isDirectory = false;
            this.file = file;
        }

        public FileStats(DirectoryInfo directory)
        {
            this.isDirectory = true;
            this.directory = directory;
        }
    }
}
