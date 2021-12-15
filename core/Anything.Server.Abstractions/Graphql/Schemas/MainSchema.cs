using System;
using Anything.Server.Abstractions.Graphql.Types;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Anything.Server.Abstractions.Graphql.Schemas;

internal class MainSchema : Schema
{
    private readonly IServiceProvider _serviceProvider;

    public MainSchema(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Build();
    }

    private void Build()
    {
        Query = _serviceProvider.GetRequiredService<QueryObject>();
        Mutation = _serviceProvider.GetRequiredService<MutationObject>();

        FieldMiddleware.Use(_serviceProvider.GetRequiredService<InstrumentFieldsMiddleware>());

        RegisterType(_serviceProvider.GetRequiredService<FileHandleRefType>());
        RegisterType(_serviceProvider.GetRequiredService<FileInterface>());
        RegisterType(_serviceProvider.GetRequiredService<RegularFileType>());
        RegisterType(_serviceProvider.GetRequiredService<UnknownFileType>());
        RegisterType(_serviceProvider.GetRequiredService<DirectoryType>());
        RegisterType(_serviceProvider.GetRequiredService<FileStatsType>());
        RegisterType(_serviceProvider.GetRequiredService<DirentType>());

        RegisterType(_serviceProvider.GetRequiredService<FileHandleGraphType>());

        RegisterType(_serviceProvider.GetRequiredService<JsonGraphType>());
        RegisterType(_serviceProvider.GetRequiredService<UrlGraphType>());
    }
}
