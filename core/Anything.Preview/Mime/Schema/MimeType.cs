namespace Anything.Preview.Mime.Schema
{
    public sealed partial record MimeType
    {
        public MimeType(string mime)
        {
            Mime = mime;
        }

        public string Mime { get; }

        public override string ToString()
        {
            return Mime;
        }
    }
}
