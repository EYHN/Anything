using Anything.Server.Abstractions.Graphql.Models;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class FileHandleRefType : ObjectGraphType<FileHandleRefEntry>
{
    public FileHandleRefType()
    {
        Name = "FileHandleRef";
        Description = "A reference to a file handle.";

        Field<NonNullGraphType<FileHandleGraphType>>(
            "value",
            resolve: d => d.Source!.Value,
            description: "The file handle.");

        FieldAsync<NonNullGraphType<DirectoryType>>(
            "openDirectory",
            resolve: async d => await d.Source!.OpenDirectory(),
            description: "Open file handle as a directory.");

        FieldAsync<NonNullGraphType<FileInterface>>(
            "openFile",
            resolve: async d => await d.Source!.OpenFile(),
            description: "Open file handle as a file.");
    }
}
