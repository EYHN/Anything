using System.Collections.Generic;
using Anything.Server.Abstractions.Graphql.Endpoint;
using Anything.Server.Abstractions.Graphql.Models;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class FileInterface : InterfaceGraphType<FileEntry>
{
    public FileInterface(IEnumerable<FileFieldEndpoint> fieldAddons)
    {
        Name = "File";
        Description = "A File object can represent either a file or a directory.";
        Field<NonNullGraphType<StringGraphType>>("_id", "Identifier for this file. Same as fileHandle.value.identifier");
        Field<NonNullGraphType<FileHandleRefType>>(
            "fileHandle",
            "File handle of the file.");
        Field<NonNullGraphType<StringGraphType>>(
            "name",
            "Name of the file.");
        Field<NonNullGraphType<UrlGraphType>>(
            "url",
            "Url of the file.");
        Field<NonNullGraphType<FileStatsType>>("stats", "Information about the file.");

        foreach (var field in fieldAddons)
        {
            Field(field.GraphType, field.Name, field.Description);
        }
    }
}
