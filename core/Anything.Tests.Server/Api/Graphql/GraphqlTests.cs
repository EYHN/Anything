using System;
using System.Threading.Tasks;
using Anything.Config;
using Anything.FileSystem.Impl;
using Anything.Preview;
using Anything.Preview.MimeType;
using Anything.Search;
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
            using var fileService = new MemoryFileService(Url.Parse("file://memory/"));
            var previewService = await PreviewServiceFactory.BuildPreviewService(
                fileService,
                MimeTypeRules.TestRules,
                TestUtils.GetTestDirectoryPath("cache"));
            var searchService = SearchServiceFactory.BuildSearchService(fileService, TestUtils.GetTestDirectoryPath("index"));
            using var schema = new MainSchema(new Application(configuration, fileService, previewService, searchService));

            var result = await schema.ExecuteAsync(
                _ =>
                {
                    _.Query = "{ directory(url : \"file://memory/\") { url } }";
                });

            Console.WriteLine(result);
        }
    }
}
