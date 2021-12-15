namespace Anything.Server.Abstractions.Graphql.Models;

public class DirentEntry
{
    public DirentEntry(string name, FileEntry fileEntry)
    {
        Name = name;
        FileEntry = fileEntry;
    }

    public string Name { get; }

    public FileEntry FileEntry { get; }
}
