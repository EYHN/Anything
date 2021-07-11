using System;
using System.Threading.Tasks;
using Anything.Config;
using Anything.FileSystem;
using Anything.Preview;
using Anything.Preview.MimeType;
using Anything.Search;
using Anything.Server.Api.Graphql.Schemas;
using Anything.Server.Models;
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
            var fileService = FileServiceFactory.BuildMemoryFileService();
            var previewService = await PreviewServiceFactory.BuildPreviewService(
                fileService,
                MimeTypeRules.TestRules,
                TestUtils.GetTestDirectoryPath("cache"));
            var searchService = SearchServiceFactory.BuildSearchService(fileService, TestUtils.GetTestDirectoryPath("index"));
            var schema = new MainSchema(new Application(configuration, fileService, previewService, searchService));

            var result = await schema.ExecuteAsync(
                _ =>
                {
                    _.Query = "{ directory(url : \"file://memory/\") { url } }";
                });

            Console.WriteLine(result);
        }
    }
}
