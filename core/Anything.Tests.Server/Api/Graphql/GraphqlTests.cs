using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview;
using Anything.Preview.MimeType;
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
            var fileService = await FileServiceFactory.BuildMemoryFileService();
            var previewService = await PreviewServiceFactory.BuildPreviewService(
                fileService,
                MimeTypeRules.TestRules,
                TestUtils.GetTestDirectoryPath("cache"));
            var schema = new MainSchema(new Application(fileService, previewService));

            var result = await schema.ExecuteAsync(
                _ =>
                {
                    _.Query = "{ directory(url : \"file://memory/\") { url } }";
                });

            Console.WriteLine(result);
        }
    }
}
