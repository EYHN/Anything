using System;
using System.Security.Cryptography;
using System.Text;

namespace Anything.FileSystem
{
    public record FileRecord(string IdentifierTag, string ContentTag, FileType Type)
    {
        public static FileRecord FromFileStats(FileStats fileStats)
        {
            using var sha256 = SHA256.Create();
            var identifierTag = Convert.ToHexString(
                sha256.ComputeHash(
                    Encoding.UTF8.GetBytes($"{(int)fileStats.Type})"))).Substring(0, 7);
            var contentTag = Convert.ToHexString(
                    sha256.ComputeHash(
                        Encoding.UTF8.GetBytes($"{fileStats.LastWriteTime.ToUnixTimeMilliseconds()} + '-' + {fileStats.Size}")))
                .Substring(0, 7);

            return new FileRecord(identifierTag, contentTag, fileStats.Type);
        }

        public override string ToString()
        {
            return $@"{IdentifierTag}:{ContentTag}:{(int)Type}";
        }
    }
}
