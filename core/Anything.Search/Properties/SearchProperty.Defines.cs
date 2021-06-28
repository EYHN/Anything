namespace Anything.Search.Properties
{
    public partial record SearchProperty
    {
        public static SearchProperty FileName { get; } = new("filename", DataType.Text);

        public static SearchProperty Description { get; } = new("description", DataType.Text);
    }
}
