using Anything.FileSystem;
using Anything.Server.Abstractions.Graphql;
using Anything.Server.Abstractions.Graphql.Models;
using Anything.Server.Abstractions.Graphql.Types;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Anything.Notes;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddNoteFeature(this IServiceCollection services)
    {
        services.TryAddScoped<INoteService, NoteService>();

        services.TryAddFileFieldEndpoint<StringGraphType>(
            "notes",
            "Notes of the file.",
            resolve: async (FileHandle handle, INoteService noteService) => await noteService.GetNotes(handle));

        services.TryAddMutationEndpoint<NonNullGraphType<FileInterface>>(
            "setNotes",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<FileHandleGraphType>> { Name = "fileHandle" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "notes" }),
            resolve: async (FileHandle handle, string notes, INoteService noteService, ApplicationEntry applicationEntry) =>
            {
                await noteService.SetNotes(handle, notes);
                return await applicationEntry.CreateFile(handle);
            });
        return services;
    }
}
