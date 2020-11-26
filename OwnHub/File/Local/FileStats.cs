using System;
using System.IO;
using OwnHub.File.Base;

namespace OwnHub.File.Local
{
    public class FileStats : BaseFileStats
    {
        private readonly DirectoryInfo? directory;
        private readonly FileInfo? file;
        private readonly bool isDirectory;

        public FileStats(FileInfo file)
        {
            isDirectory = false;
            this.file = file;
        }

        public FileStats(DirectoryInfo directory)
        {
            isDirectory = true;
            this.directory = directory;
        }

        private FileSystemInfo FileSystemInfo => isDirectory ? directory! : (FileSystemInfo) file!;

        public override long? Size => isDirectory ? null : (long?) file!.Length;

        public override DateTimeOffset? ModifyTime => new DateTimeOffset(FileSystemInfo.LastWriteTimeUtc);

        public override DateTimeOffset? AccessTime => new DateTimeOffset(FileSystemInfo.LastAccessTimeUtc);

        public override DateTimeOffset? CreationTime => new DateTimeOffset(FileSystemInfo.CreationTimeUtc);
    }
}