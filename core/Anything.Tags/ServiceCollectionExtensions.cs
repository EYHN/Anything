using System.Collections.Generic;
using System.Linq;
using Anything.FileSystem;
using Anything.Server.Abstractions.Graphql;
using Anything.Server.Abstractions.Graphql.Models;
using Anything.Server.Abstractions.Graphql.Types;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Anything.Tags;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddTagFeature(this IServiceCollection services)
    {
        services.TryAddScoped<ITagService, TagService>();

        services.TryAddFileFieldEndpoint<NonNullGraphType<ListGraphType<NonNullGraphType<StringGraphType>>>>(
            "tags",
            "Tags of the file.",
            resolve: async (FileHandle handle, ITagService tagService) => (await tagService.GetTags(handle)).Select(t => t.Name));

        services.TryAddMutationEndpoint<NonNullGraphType<FileInterface>>(
            "setTags",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<FileHandleGraphType>> { Name = "fileHandle" },
                new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<StringGraphType>>>> { Name = "tags" }),
            resolve: async (FileHandle handle, List<string> tags, ITagService tagService, ApplicationEntry applicationEntry) =>
            {
                await tagService.SetTags(handle, tags.Select(t => new Tag(t)));
                return await applicationEntry.CreateFile(handle);
            });
        return services;
    }
}
