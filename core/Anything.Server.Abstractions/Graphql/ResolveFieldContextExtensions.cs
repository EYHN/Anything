using Anything.Server.Abstractions.Graphql.Models;
using GraphQL;
using Microsoft.Extensions.DependencyInjection;

namespace Anything.Server.Abstractions.Graphql;

internal static class ResolveFieldContextExtensions
{
    public static ApplicationEntry GetApplication<T>(this IResolveFieldContext<T> context)
    {
        return (ApplicationEntry)context.RequestServices!.GetRequiredService(typeof(ApplicationEntry))!;
    }
}
