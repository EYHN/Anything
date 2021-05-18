using System.IO;
using System.Text.Json;
using StagingBox.Utils;

namespace StagingBox.Preview.MimeType
{
    public class MimeTypeRules
    {
        public record MimeTypeRule(string Mime, string[] Extensions, string Icon);

        private readonly MimeTypeRule[] _rules;

        public MimeTypeRules(MimeTypeRule[] rules)
        {
            _rules = rules;
        }

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

        public string? Match(Url url)
        {
            var extname = PathLib.Extname(url.Path).ToLower();
            foreach (MimeTypeRule mimetype in _rules)
            {
                foreach (var ext in mimetype.Extensions)
                {
                    if (ext == extname)
                    {
                        return mimetype.Mime;
                    }
                }
            }

            return null;
        }
    }
}
