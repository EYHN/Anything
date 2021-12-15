using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Anything.Scheduling;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddBackgroundThread<TBackgroundThread>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TBackgroundThread : IBackgroundThread
    {
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IBackgroundThread), typeof(TBackgroundThread), lifetime));
        return services;
    }
}
