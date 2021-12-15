using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;

namespace Anything.Utils;

public static class Resources
{
    public static byte[] ReadEmbeddedFile(Assembly assembly, string path)
    {
        var embeddedFileProvider = new EmbeddedFileProvider(assembly);
        var fileInfo = embeddedFileProvider.GetFileInfo(path);
        using var readStream = fileInfo.CreateReadStream();
        using var memoryStream = new MemoryStream((int)readStream.Length);
        readStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static string ReadEmbeddedTextFile(Assembly assembly, string path)
    {
        var embeddedFileProvider = new EmbeddedFileProvider(assembly);
        var fileInfo = embeddedFileProvider.GetFileInfo(path);
        using var streamReader = new StreamReader(fileInfo.CreateReadStream());
        return streamReader.ReadToEnd();
    }

    public static JsonDocument ReadEmbeddedJsonFile(Assembly assembly, string path)
    {
        var options = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip };
        var document = JsonDocument.Parse(ReadEmbeddedFile(assembly, path), options);
        return document;
    }

    public static TValue ReadEmbeddedJsonFile<TValue>(Assembly assembly, string path)
    {
        var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true };
        var json = JsonSerializer.Deserialize<TValue>(
            ReadEmbeddedTextFile(assembly, path),
            options);
        return json!;
    }

    public static IEnumerable<string> GetAllEmbeddedFile(Assembly assembly, string rootPath = "/")
    {
        var embeddedFileProvider = new EmbeddedFileProvider(assembly);
        var directoryContents = embeddedFileProvider.GetDirectoryContents("/");
        rootPath = rootPath.TrimStart('/').Replace('/', '.').Replace('\\', '.');
        return directoryContents
            .Where(content => content.Name.StartsWith(rootPath, StringComparison.Ordinal))
            .Select(content => content.Name);
    }
}
