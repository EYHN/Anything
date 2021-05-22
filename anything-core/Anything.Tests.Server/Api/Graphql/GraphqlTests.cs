using System;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview;
using Anything.Preview.MimeType;
using Anything.Server.Api.Graphql.Schemas;
using Anything.Server.Models;
using GraphQL.SystemTextJson;
using NUnit.Framework;
using Anything.Preview.Icons;
using Anything.Preview.Thumbnails;

namespace Anything.Tests.Server.Api.Graphql
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
