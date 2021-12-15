using System;
using Microsoft.Extensions.DependencyInjection;

namespace Anything.Server.Abstractions.Http;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection TryAddHttpEndpoint(
        this IServiceCollection services,
        HttpEndpoint fieldEndpoint)
    {
        foreach (var service in services)
        {
            if (service.ServiceType == typeof(HttpEndpoint))
            {
                var endpoint = (HttpEndpoint)service.ImplementationInstance!;
                if (endpoint.Pattern == fieldEndpoint.Pattern && endpoint.Method == fieldEndpoint.Method)
                {
                    return services;
                }
            }
        }

        services.AddSingleton(fieldEndpoint);
        return services;
    }

    public static IServiceCollection TryAddGetEndpoint(this IServiceCollection service, string pattern, Delegate requestDelegate)
    {
        return TryAddHttpEndpoint(service, new HttpEndpoint(pattern, "GET", requestDelegate));
    }

    public static IServiceCollection TryAddPostEndpoint(this IServiceCollection service, string pattern, Delegate requestDelegate)
    {
        return TryAddHttpEndpoint(service, new HttpEndpoint(pattern, "POST", requestDelegate));
    }

    public static IServiceCollection TryAddPutEndpoint(this IServiceCollection service, string pattern, Delegate requestDelegate)
    {
        return TryAddHttpEndpoint(service, new HttpEndpoint(pattern, "PUT", requestDelegate));
    }

    public static IServiceCollection TryAddDeleteEndpoint(this IServiceCollection service, string pattern, Delegate requestDelegate)
    {
        return TryAddHttpEndpoint(service, new HttpEndpoint(pattern, "DELETE", requestDelegate));
    }
}
