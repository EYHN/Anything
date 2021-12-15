using System.Collections.Generic;
using System.Threading.Tasks;
using Anything.Server.Abstractions.Graphql.Endpoint;
using Anything.Server.Abstractions.Graphql.Models;
using GraphQL.Types;

namespace Anything.Server.Abstractions.Graphql.Types;

public class UnknownFileType : ObjectGraphType<UnknownFileEntry>
{
    public UnknownFileType(IEnumerable<FileFieldEndpoint> fieldAddons)
    {
        Name = "UnknownFile";
        Description =
            "A file whose type is not supported.";

        Field<NonNullGraphType<StringGraphType>>(
            "_id",
            resolve: d => d.Source!.FileHandle.Value.Identifier,
            description: "Identifier for this file. Same as fileHandle.value.identifier");
        Field<NonNullGraphType<FileHandleRefType>>(
            "fileHandle",
            resolve: d => d.Source!.FileHandle,
            description: "File handle of the file.");
        FieldAsync<NonNullGraphType<StringGraphType>>(
            "name",
            resolve: async d => await d.Source!.GetFileName(),
            description: "Name of the file.");
        FieldAsync<NonNullGraphType<UrlGraphType>>(
            "url",
            resolve: async d => await d.Source!.GetUrl(),
            description: "Url of the file.");
        Field<NonNullGraphType<FileStatsType>>(
            "stats",
            resolve: d => d.Source!.Stats,
            description: "Information about the file.");

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

        IsTypeOf = obj => obj is UnknownFileEntry;
    }
}
