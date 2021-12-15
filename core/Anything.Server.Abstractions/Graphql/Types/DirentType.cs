using Anything.Server.Abstractions.Graphql.Models;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class DirentType : ObjectGraphType<DirentEntry>
{
    public DirentType()
    {
        Name = "Dirent";
        Description = "A representation of a directory entry, which can be a file or a subdirectory within the directory.";

        Field<NonNullGraphType<StringGraphType>>(
            "name",
            resolve: d => d.Source!.Name,
            description: "The file name that this dirent refers to.");

        Field<NonNullGraphType<FileInterface>>(
            "file",
            resolve: d => d.Source!.FileEntry,
            description: "The file object that this dirent refers to.");

        IsTypeOf = o => o is DirentEntry;
    }
}
