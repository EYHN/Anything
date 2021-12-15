using Anything.FileSystem;
using Anything.Search.Crawlers;
using Anything.Search.Indexers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Anything.Search;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddSearchFeature(this IServiceCollection services)
    {
        services.TryAddSingleton<ISearchIndexer, LuceneRamIndexer>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISearchCrawler, FileNameSearchCrawler>());

        services.TryAddScoped<ISearchService, SearchService>();
        services.TryAddFileEventHandler<SearchService.FileEventHandler>();
        return services;
    }
}
