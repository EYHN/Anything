using System;
using System.Threading.Tasks;
using Anything.Config;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Notes;
using Anything.Preview;
using Anything.Preview.Mime;
using Anything.Search;
using Anything.Search.Indexers;
using Anything.Server.Api.Graphql.Schemas;
using Anything.Server.Models;
using Anything.Tags;
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
            var memoryFileSystem = new MemoryFileSystem();
            await memoryFileSystem.CreateFile(
                await memoryFileSystem.CreateFileHandle("/"),
                "/test.png",
                new byte[] { 0x31, 0x32, 0x33 });

            using var previewCacheStorage = new PreviewMemoryCacheStorage(fileService);
            var previewService = new PreviewService(
                fileService,
                MimeTypeRules.TestRules,
                previewCacheStorage);

            using var tagStorage = new TagService.MemoryStorage();
            using var tagService = new TagService(fileService, tagStorage);

            using var noteStorage = new NoteService.MemoryStorage();
            using var noteService = new NoteService(fileService, noteStorage);

            using var searchIndexer = new LuceneIndexer();
            using var searchService = SearchServiceFactory.BuildSearchService(fileService, searchIndexer);
            using var schema =
                new MainSchema();

            fileService.AddFileSystem(
                "memory",
                memoryFileSystem);

            var result = await schema.ExecuteAsync(
                _ =>
                {
                    _.Query = $@"{{
  createFileHandle(url : ""file://memory/"") {{
    openDirectory {{
      __typename
      fileHandle {{
        value
      }}
      icon
      mime
      icon
      thumbnail
      metadata
      tags
      stats {{
        creationTime
        lastWriteTime
        size
      }}
      entries {{
        name
        file {{
          __typename
          fileHandle {{
            value
          }}
          icon
          mime
          icon
          thumbnail
          metadata
          tags
          stats {{
            creationTime
            lastWriteTime
            size
          }}
        }}
      }}
    }}
  }}
}}";
                    _.RequestServices =
                        new TestServiceProvider(new Application(configuration, fileService, previewService, searchService, tagService, noteService));
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
