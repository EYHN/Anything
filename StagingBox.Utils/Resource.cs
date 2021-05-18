using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;

namespace StagingBox.Utils
{
    public class Resources
    {
        public static Stream ReadEmbeddedFile(Assembly assembly, string path)
        {
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            var fileInfo = embeddedFileProvider.GetFileInfo(path);
            return fileInfo.CreateReadStream();
        }

        public static string ReadEmbeddedTextFile(Assembly assembly, string path)
        {
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            var fileInfo = embeddedFileProvider.GetFileInfo(path);
            return new StreamReader(fileInfo.CreateReadStream()).ReadToEnd();
        }

        public static JsonDocument ReadEmbeddedJsonFile(Assembly assembly, string path)
        {
            var options = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip
            };
            var document = JsonDocument.Parse(ReadEmbeddedFile(assembly, path), options);
            return document;
        }

        public static TValue ReadEmbeddedJsonFile<TValue>(Assembly assembly, string path)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
            var json = JsonSerializer.Deserialize<TValue>(
                ReadEmbeddedTextFile(assembly, path),
                options
            );
            return json!;
        }
    }
}
