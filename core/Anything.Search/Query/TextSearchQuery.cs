using Anything.Search.Properties;

namespace Anything.Search.Query
{
    public class TextSearchQuery : SearchQuery
    {
        public TextSearchQuery(SearchProperty property, string text)
        {
            Property = property;
            Text = text;
        }

        public SearchProperty Property { get; }

        public string Text { get; }
    }
}
