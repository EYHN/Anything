using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public class MimeType
    {
        public string Mime { get; set; }
        public string[] Extensions { get; set; }
        public string icon { get; set; }
    }

    public class MimeTypeRules
    {
        MimeType[] rules;

        public static MimeTypeRules DefaultRules = FromJSON(System.IO.File.ReadAllText(Path.Join(Utils.Utils.GetApplicationRoot(), "Resources/mimetype.json")));
        public MimeTypeRules(MimeType[] rules)
        {
            this.rules = rules;
        }

        public static MimeTypeRules FromJSON(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            MimeType[] rules = JsonSerializer.Deserialize<MimeType[]>(json, options);
            return new MimeTypeRules(rules);
        }

        public MimeType Match(string extname)
        {
            extname = extname.Replace(".", "").ToLower();
            foreach (var mimetype in this.rules)
            {
                foreach(var ext in mimetype.Extensions)
                {
                    if (ext == extname)
                    {
                        return mimetype;
                    }
                }
            }
            return null;
        }
    }
}
