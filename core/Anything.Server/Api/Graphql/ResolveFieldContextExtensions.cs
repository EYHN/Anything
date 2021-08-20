using Anything.Server.Models;
using GraphQL;

namespace Anything.Server.Api.Graphql
{
    internal static class ResolveFieldContextExtensions
    {
        public static Application GetApplication<T>(this IResolveFieldContext<T> context)
        {
            return (Application)context.RequestServices.GetService(typeof(Application))!;
        }
    }
}
