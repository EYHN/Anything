using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using Anything.Preview.Mime.Schema;
using Anything.Utils;

namespace Anything.Preview.Mime
{
    public class MimeTypeRules
    {
        private readonly Dictionary<string, string> _extensionMimeMap = new();

        public MimeTypeRules(MimeTypeRule[] rules)
        {
            foreach (var rule in rules)
            {
                foreach (var extension in rule.Extensions)
                {
                    _extensionMimeMap.Add(extension, rule.Mime);
                }
            }
        }

        public static MimeTypeRules DefaultRules =>
            FromJson(Resources.ReadEmbeddedTextFile(typeof(MimeTypeRule).Assembly, "/Resources/mimetype.json"));

        public static MimeTypeRules TestRules => FromJson(
            "[{\"mime\":\"image/png\",\"extensions\":[\".png\"]},{\"mime\":\"image/jpeg\",\"extensions\":[\".jpg\",\".jpeg\",\".jpe\"]},{\"mime\":\"image/bmp\",\"extensions\":[ \".bmp\"]}]");

        public static MimeTypeRules FromJson(string json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip };
            var rules = JsonSerializer.Deserialize<MimeTypeRule[]>(json, options);
            if (rules == null)
            {
                throw new InvalidDataException("JSON data error.");
            }

            return new MimeTypeRules(rules);
        }

        public MimeType? Match(Url url)
        {
            var extname = PathLib.Extname(url.Path).ToLowerInvariant();

            return _extensionMimeMap.TryGetValue(extname, out var mime) ? new MimeType(mime) : null;
        }

        public record MimeTypeRule(string Mime, ImmutableArray<string> Extensions);
    }
}
