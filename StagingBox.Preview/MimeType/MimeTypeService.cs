using System.Threading.Tasks;
using StagingBox.Utils;

namespace StagingBox.Preview.MimeType
{
    public class MimeTypeService : IMimeTypeService
    {
        private readonly MimeTypeRules _rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeTypeService"/> class.
        /// </summary>
        /// <param name="rules">Mime type rules.</param>
        public MimeTypeService(MimeTypeRules rules)
        {
            _rules = rules;
        }

        /// <inheritdoc/>
        public ValueTask<string?> GetMimeType(Url url, MimeTypeOption option)
        {
            return ValueTask.FromResult(_rules.Match(url));
        }
    }
}
