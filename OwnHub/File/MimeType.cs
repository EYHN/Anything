using System.IO;
using System.Text.Json;

namespace OwnHub.File
{
    public struct MimeType
    {
        public string Mime { get; set; }
        public string[] Extensions { get; set; }
        public string Icon { get; set; }
    }

    public class MimeTypeRules
    {
        public static MimeTypeRules
            DefaultRules = FromJson(Utils.Utils.ReadEmbeddedTextFile("Resources/mimetype.json"));

        private readonly MimeType[] rules;

        public MimeTypeRules(MimeType[] rules)
        {
            this.rules = rules;
        }

        public static MimeTypeRules FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            MimeType[]? rules = JsonSerializer.Deserialize<MimeType[]>(json, options);
            if (rules == null) throw new InvalidDataException("JSON data error.");
            return new MimeTypeRules(rules);
        }

        public MimeType? Match(string extname)
        {
            extname = extname.Replace(".", "").ToLower();
            foreach (MimeType mimetype in rules)
            foreach (var ext in mimetype.Extensions)
                if (ext == extname)
                    return mimetype;
            return null;
        }
    }
}