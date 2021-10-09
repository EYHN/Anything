namespace Anything.Server.Models
{
    public class Dirent
    {
        public string Name { get; }

        public File File { get; }

        public Dirent(string name, File file)
        {
            Name = name;
            File = file;
        }
    }
}
