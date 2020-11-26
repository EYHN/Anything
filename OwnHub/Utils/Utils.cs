using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;

namespace OwnHub.Utils
{
    public class Utils
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        private static readonly EmbeddedFileProvider
            embeddedFileProvider = new EmbeddedFileProvider(assembly, "OwnHub");

        public static string? GetApplicationRoot()
        {
            return Path.GetDirectoryName(assembly.Location);
        }

        public static Stream ReadEmbeddedFile(string path)
        {
            IFileInfo? fileInfo = embeddedFileProvider.GetFileInfo(path);
            return fileInfo.CreateReadStream();
        }

        public static string ReadEmbeddedTextFile(string path)
        {
            IFileInfo? fileInfo = embeddedFileProvider.GetFileInfo(path);
            return new StreamReader(fileInfo.CreateReadStream()).ReadToEnd();
        }

        public static TValue DeserializeEmbeddedJsonFile<TValue>(string path)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
            var json = JsonSerializer.Deserialize<TValue>(
                ReadEmbeddedTextFile(path),
                options
            );
            return json;
        }
    }
}