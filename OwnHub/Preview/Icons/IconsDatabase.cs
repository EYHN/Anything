using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MoreLinq;
using OwnHub.Utils;

namespace OwnHub.Preview.Icons
{
    public class IconsDatabase
    {
        public static string CalcFileEtag(DateTimeOffset modifyTime, long size)
        {
            byte[] data = Encoding.UTF8.GetBytes(modifyTime.ToUnixTimeMilliseconds() + size.ToString());
            byte[] hash = SHA256.Create().ComputeHash(data);

            string hex = BitConverter.ToString(hash).Replace("-", "");
            return hex;
        }
    }
}
