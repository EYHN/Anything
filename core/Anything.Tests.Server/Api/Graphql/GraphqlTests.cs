using System;
using System.Threading.Tasks;
using Anything.Config;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.FileSystem.Provider;
using Anything.Preview;
using Anything.Preview.Mime;
using Anything.Search;
using Anything.Search.Indexers;
using Anything.Server.Api.Graphql.Schemas;
using Anything.Server.Models;
using Anything.Utils;
using GraphQL.SystemTextJson;
using NUnit.Framework;

namespace Anything.Tests.Server.Api.Graphql
{
    public class GraphqlTests
    {
        [Test]
        public async Task GraphqlSchemaTest()
        {
            var configuration = ConfigurationFactory.BuildDevelopmentConfiguration();
            using var fileService = new FileService();
            var memoryFileSystemProvider = new MemoryFileSystemProvider();
            await memoryFileSystemProvider.WriteFile(Url.Parse("file://memory/test.txt"), new byte[] { 0x31, 0x32, 0x33 });
            fileService.AddFileSystem(
                Url.Parse("file://memory/"),
                new ReadonlyStaticFileSystem(Url.Parse("file://memory/"), memoryFileSystemProvider));

            using var previewCacheStorage = new PreviewMemoryCacheStorage();
            using var previewService = new PreviewService(
                fileService,
                MimeTypeRules.TestRules,
                previewCacheStorage);

            using var searchIndexer = new LuceneIndexer();
            using var searchService = SearchServiceFactory.BuildSearchService(fileService, searchIndexer);
            using var schema =
                new MainSchema();

            var result = await schema.ExecuteAsync(
                _ =>
                {
                    _.Query = $@"{{
  directory(url : ""file://memory/"") {{
    __typename
    url
    name
    icon
    mime
    icon
    thumbnail
    metadata
    stats {{
      creationTime
      lastWriteTime
      size
    }}
    entries {{
      __typename
      url
      name
      icon
      mime
      icon
      thumbnail
      metadata
      stats {{
        creationTime
        lastWriteTime
        size
      }}
    }}
  }}
}}";
                    _.RequestServices = new TestServiceProvider(new Application(configuration, fileService, previewService, searchService));
                });

            Console.WriteLine(result);
        }

        private class TestServiceProvider : IServiceProvider
        {
            private readonly Application _application;

            public TestServiceProvider(Application application)
            {
                _application = application;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == null)
                {
                    throw new ArgumentNullException(nameof(serviceType));
                }

                if (serviceType == typeof(Application))
                {
                    return _application;
                }

                try
                {
                    return Activator.CreateInstance(serviceType);
                }
                catch (Exception exception)
                {
                    throw new AggregateException($"Failed to call Activator.CreateInstance. Type: {serviceType.FullName}", exception);
                }
            }
        }
    }
}
