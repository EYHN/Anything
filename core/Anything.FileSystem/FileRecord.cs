using System;
using System.Security.Cryptography;
using System.Text;

namespace Anything.FileSystem
{
    public record FileRecord(string IdentifierTag, string ContentTag, FileType Type)
    {
        public static FileRecord FromFileStats(FileStats fileStats)
        {
            var identifierTag = Convert.ToHexString(
                SHA256.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(((int)fileStats.Type).ToString()))).Substring(0, 7);
            var contentTag = Convert.ToHexString(
                    SHA256.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(fileStats.LastWriteTime.ToUnixTimeMilliseconds() + '-' + fileStats.Size.ToString())))
                .Substring(0, 7);

            return new FileRecord(identifierTag, contentTag, fileStats.Type);
        }

        public static FileRecord FromString(string recordString)
        {
            var splited = recordString.Split(':');
            return new FileRecord(splited[0], splited[1], (FileType)Convert.ToInt32(splited[2]));
        }

        public override string ToString()
        {
            return $@"{IdentifierTag}:{ContentTag}:{(int)Type}";
        }
    }
}
