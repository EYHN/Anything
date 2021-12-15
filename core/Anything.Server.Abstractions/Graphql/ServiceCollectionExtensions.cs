using System;
using Anything.Server.Abstractions.Graphql.Endpoint;
using Anything.Server.Abstractions.Graphql.Models;
using Anything.Server.Abstractions.Graphql.Schemas;
using Anything.Server.Abstractions.Graphql.Types;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Anything.Server.Abstractions.Graphql;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection TryAddFileFieldEndpoint(
        this IServiceCollection services,
        FileFieldEndpoint fieldEndpoint)
    {
        foreach (var service in services)
        {
            if (service.ServiceType == typeof(FileFieldEndpoint))
            {
                var endpoint = (FileFieldEndpoint)service.ImplementationInstance!;
                if (endpoint.Name == fieldEndpoint.Name)
                {
                    return services;
                }
            }
        }

        services.AddSingleton(fieldEndpoint);
        return services;
    }

    public static IServiceCollection TryAddFileFieldEndpoint<TGraphType>(
        this IServiceCollection services,
        string name,
        string? description = null,
        QueryArguments? arguments = null,
        Delegate? resolve = null)
        where TGraphType : IGraphType
    {
        resolve ??= new Func<object?>(() => null);
        return services.TryAddFileFieldEndpoint(new FileFieldEndpoint(name, description, typeof(TGraphType), resolve, arguments));
    }

    private static IServiceCollection TryAddQueryEndpoint(
        this IServiceCollection services,
        QueryEndpoint queryEndpoint)
    {
        foreach (var service in services)
        {
            if (service.ServiceType == typeof(QueryEndpoint))
            {
                var endpoint = (QueryEndpoint)service.ImplementationInstance!;
                if (endpoint.Name == queryEndpoint.Name)
                {
                    return services;
                }
            }
        }

        services.AddSingleton(queryEndpoint);
        return services;
    }

    public static IServiceCollection TryAddQueryEndpoint<TGraphType>(
        this IServiceCollection services,
        string name,
        string? description = null,
        QueryArguments? arguments = null,
        Delegate? resolve = null)
        where TGraphType : IGraphType
    {
        resolve ??= new Func<object?>(() => null);
        return services.TryAddQueryEndpoint(new QueryEndpoint(name, description, typeof(TGraphType), resolve, arguments));
    }

    private static IServiceCollection TryAddMutationEndpoint(
        this IServiceCollection services,
        MutationEndpoint mutationEndpoint)
    {
        foreach (var service in services)
        {
            if (service.ServiceType == typeof(MutationEndpoint))
            {
                var endpoint = (MutationEndpoint)service.ImplementationInstance!;
                if (endpoint.Name == mutationEndpoint.Name)
                {
                    return services;
                }
            }
        }

        services.AddSingleton(mutationEndpoint);
        return services;
    }

    public static IServiceCollection TryAddMutationEndpoint<TGraphType>(
        this IServiceCollection services,
        string name,
        string? description = null,
        QueryArguments? arguments = null,
        Delegate? resolve = null)
        where TGraphType : IGraphType
    {
        resolve ??= new Func<object?>(() => null);
        return services.TryAddMutationEndpoint(new MutationEndpoint(name, description, typeof(TGraphType), resolve, arguments));
    }

    public static IServiceCollection AddGraphQlTypes(this IServiceCollection services)
    {
        services.TryAddSingleton<ApplicationEntry>();

        services.TryAddSingleton<MainSchema>();
        services.TryAddSingleton<ISchema, MainSchema>();

        services.TryAddSingleton<MutationObject>();
        services.TryAddSingleton<QueryObject>();

        services.TryAddSingleton<FileHandleRefType>();
        services.TryAddSingleton<FileInterface>();
        services.TryAddSingleton<RegularFileType>();
        services.TryAddSingleton<UnknownFileType>();
        services.TryAddSingleton<DirectoryType>();
        services.TryAddSingleton<FileStatsType>();
        services.TryAddSingleton<DirentType>();

        services.TryAddSingleton<FileHandleGraphType>();

        services.TryAddSingleton<JsonGraphType>();
        services.TryAddSingleton<UrlGraphType>();
        return services;
    }
}
