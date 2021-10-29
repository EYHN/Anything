using System.Collections.Generic;
using System.Linq;
using Anything.FileSystem;
using Anything.Server.Api.Graphql.Types;
using Anything.Tags;
using GraphQL;
using GraphQL.Types;

namespace Anything.Server.Api.Graphql.Schemas
{
    public class MutationObject : ObjectGraphType<object>
    {
        public MutationObject()
        {
            Name = "Mutation";
            Description = "The mutation type, represents all updates we can make to our data.";

            FieldAsync<NonNullGraphType<FileInterface>>(
                "addTags",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<FileHandleGraphType>> { Name = "fileHandle" },
                    new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<StringGraphType>>>> { Name = "tags" }),
                resolve: async context =>
                {
                    var fileHandle = context.GetArgument<FileHandle>("fileHandle");
                    var tags = context.GetArgument<List<string>>("tags");

                    return await context.GetApplication().AddTags(fileHandle, tags.Select(t => new Tag(t)).ToArray());
                });

            FieldAsync<NonNullGraphType<FileInterface>>(
                "removeTags",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<FileHandleGraphType>> { Name = "fileHandle" },
                    new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<StringGraphType>>>> { Name = "tags" }),
                resolve: async context =>
                {
                    var fileHandle = context.GetArgument<FileHandle>("fileHandle");
                    var tags = context.GetArgument<List<string>>("tags");

                    return await context.GetApplication().RemoveTags(fileHandle, tags.Select(t => new Tag(t)).ToArray());
                });

            FieldAsync<NonNullGraphType<FileInterface>>(
                "setNotes",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<FileHandleGraphType>> { Name = "fileHandle" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "notes" }),
                resolve: async context =>
                {
                    var fileHandle = context.GetArgument<FileHandle>("fileHandle");
                    var notes = context.GetArgument<string>("notes");

                    return await context.GetApplication().SetNotes(fileHandle, notes);
                });
        }
    }
}
