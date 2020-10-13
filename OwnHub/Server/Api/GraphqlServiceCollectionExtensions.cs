using GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;
using OwnHub.Server.Api.Graphql.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.Server.Api
{
    static class GraphqlServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQLService(this IServiceCollection services)
        {
            const string Environment = "Development";
            services
                .AddSingleton<JsonGraphType>()
                .AddGraphQL(
                    options =>
                    {
                        var complexityConfiguration = new GraphQL.Validation.Complexity.ComplexityConfiguration();
                        complexityConfiguration.MaxComplexity = 250;
                        complexityConfiguration.MaxDepth = 15;
                        // Set some limits for security, read from configuration.
                        options.ComplexityConfiguration = complexityConfiguration;
                        // Enable GraphQL metrics to be output in the response, read from configuration.
                        options.EnableMetrics = Environment == "Development";
                        // Show stack traces in exceptions. Don't turn this on in production.
                        options.ExposeExceptions = Environment == "Development";
                    })
                    .AddSystemTextJson(deserializerSettings => { }, serializerSettings => { })
                    .AddGraphTypes();
            return services;
        }
    }
}
