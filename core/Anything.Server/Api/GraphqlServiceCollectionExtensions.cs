using Anything.Server.Abstractions.Graphql;
using GraphQL.Server;
using GraphQL.Validation.Complexity;
using Microsoft.Extensions.DependencyInjection;

namespace Anything.Server.Api;

internal static class GraphqlServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQlService(this IServiceCollection services)
    {
        const string environment = "Development";
        services
            .AddGraphQlTypes()
            .AddGraphQL(
                options =>
                {
                    var complexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 };

                    // Set some limits for security, read from configuration.
                    options.ComplexityConfiguration = complexityConfiguration;

                    // Enable GraphQL metrics to be output in the response, read from configuration.
                    options.EnableMetrics = environment == "Development";
                })
            .AddSystemTextJson(_ => { }, _ => { })
            .AddErrorInfoProvider(opt => opt.ExposeExceptionStackTrace = environment == "Development");
        return services;
    }
}
