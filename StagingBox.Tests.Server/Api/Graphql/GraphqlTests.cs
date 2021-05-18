using System;
using System.Threading.Tasks;
using GraphQL.SystemTextJson;
using NUnit.Framework;
using StagingBox.FileSystem;
using StagingBox.FileSystem.Provider;
using StagingBox.Preview;
using StagingBox.Preview.Icons;
using StagingBox.Preview.MimeType;
using StagingBox.Preview.Thumbnails;
using StagingBox.Server.Api.Graphql.Schemas;
using StagingBox.Server.Models;

namespace StagingBox.Tests.Server.Api.Graphql
{
    public class GraphqlTests
    {
        [Test]
        public async Task GraphqlSchemaTest()
        {
            var fileSystemService = new VirtualFileSystemService();
            fileSystemService.RegisterFileSystemProvider("memory", new MemoryFileSystemProvider());
            var previewService = await PreviewServiceFactory.BuildPreviewService(
                fileSystemService,
                MimeTypeRules.TestRules,
                TestUtils.GetTestDirectoryPath("cache"));
            var schema = new MainSchema(new Application(fileSystemService, previewService));

            var result = await schema.ExecuteAsync(
                _ =>
                {
                    _.Query = "{ directory(url : \"file://memory/\") { url } }";
                });

            Console.WriteLine(result);
        }
    }
}
