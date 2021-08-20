namespace Anything.Preview.MimeType.Schema
{
    public sealed partial record MimeType
    {
        public string Mime { get; }

        public MimeType(string mime)
        {
            Mime = mime;
        }

        public override string ToString()
        {
            return Mime;
        }
    }
}
