using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OwnHub.Utils
{
    public class Utils
    {
        static System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        static EmbeddedFileProvider embeddedFileProvider = new EmbeddedFileProvider(assembly, "OwnHub");

        public static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(assembly.Location);
        }

        public static Stream ReadEmbeddedFile(string path)
        {
            var fileinfo = embeddedFileProvider.GetFileInfo(path);
            return fileinfo.CreateReadStream();
        }

        public static string ReadEmbeddedTextFile(string path)
        {
            var fileinfo = embeddedFileProvider.GetFileInfo(path);
            return new StreamReader(fileinfo.CreateReadStream()).ReadToEnd();
        }

        public static TValue DeserializeEmbeddedJsonFile<TValue>(string path)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
            TValue Json = JsonSerializer.Deserialize<TValue>(
                ReadEmbeddedTextFile(path),
                options
                );
            return Json;
        }
    }
}
