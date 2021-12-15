using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anything.Server.Abstractions.Graphql.Endpoint;
using Anything.Server.Abstractions.Graphql.Models;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class DirectoryType : ObjectGraphType<DirectoryEntry>
{
    public DirectoryType(IEnumerable<FileFieldEndpoint> fieldAddons)
    {
        Name = "Directory";
        Description = "A directory is a location for storing files.";

        Field<NonNullGraphType<StringGraphType>>(
            "_id",
            resolve: d => d.Source!.FileHandle.Value.Identifier,
            description: "Identifier for this directory. Same as fileHandle.value.identifier");
        Field<NonNullGraphType<FileHandleRefType>>(
            "fileHandle",
            resolve: d => d.Source!.FileHandle,
            description: "File handle of the directory.");
        FieldAsync<NonNullGraphType<StringGraphType>>(
            "name",
            resolve: async d => await d.Source!.GetFileName(),
            description: "Name of the directory.");
        FieldAsync<NonNullGraphType<UrlGraphType>>(
            "url",
            resolve: async d => await d.Source!.GetUrl(),
            description: "Url of the directory.");
        Field<NonNullGraphType<FileStatsType>>(
            "stats",
            resolve: d => d.Source!.Stats,
            description: "Information about the directory.");
        FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<DirentType>>>>(
            "entries",
            resolve: async d => (await d.Source!.ReadEntries()).ToArray(),
            description: "Entries of the directory.");

        foreach (var addon in fieldAddons)
        {
            if (addon.IsAsync)
            {
                FieldAsync(
                    addon.GraphType,
                    addon.Name,
                    addon.Description,
                    addon.Arguments,
                    context => (Task<object?>)(addon.Resolve(context.Source!.FileHandle.Value, context, context.RequestServices!)!));
            }
            else
            {
                Field(
                    addon.GraphType,
                    addon.Name,
                    addon.Description,
                    addon.Arguments,
                    context => addon.Resolve(context.Source!.FileHandle.Value, context, context.RequestServices!));
            }
        }

        Interface<FileInterface>();

        IsTypeOf = obj => obj is DirectoryEntry;
    }
}
