using OwnHub.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Preview.Icons
{
    public static class IconsFileExtensions
    {
        public static string GetIcon(this IFile file)
        {
            if (file is IRegularFile)
            {
                var mime = ((IFile)file).MimeType;
                return mime?.icon ?? "regular_file";
            }
            if (file is IDirectory)
            {
                return "directory";
            }

            return "unknown_file";
        }
    }
}
