using System;
using System.Security.Cryptography;
using System.Text;

namespace OwnHub.FileSystem
{
    public record FileStat(DateTimeOffset CreationTime, DateTimeOffset LastWriteTime, long Size, FileType Type)
    {
        /// <summary>
        /// Convert to file record.
        /// </summary>
        public FileRecord ToFileRecord()
        {
            string identifierTag = Convert.ToHexString(
                SHA256.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(((int)Type).ToString())));
            string contentTag = Convert.ToHexString(
                SHA256.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(LastWriteTime.ToUnixTimeMilliseconds() + '-' + Size.ToString())));

            return new FileRecord(identifierTag, contentTag, Type, LastWriteTime);
        }
    }
}
