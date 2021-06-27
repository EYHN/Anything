namespace Anything.Search.Properties
{
    public partial class SearchProperty
    {
        public enum DataType
        {
            Text
        }

        private SearchProperty(string name, DataType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public DataType Type { get; }
    }
}
