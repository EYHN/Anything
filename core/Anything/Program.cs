using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Anything.Config;
using Anything.Database;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview;
using Anything.Preview.Mime;
using Anything.Search;
using Anything.Search.Indexers;
using Anything.Server.Models;
using Anything.Utils;

namespace Anything
{
    public static class Program
    {
        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description = "My sample app",
                Handler = CommandHandler.Create(
                    () =>
                    {
                        Task.Run(
                            async () =>
                            {
                                var configuration = ConfigurationFactory.BuildDevelopmentConfiguration();

                                var fileService = new FileService();

                                using var previewCacheStorage = new PreviewMemoryCacheStorage();
                                var previewService = await PreviewServiceFactory.BuildPreviewService(
                                    fileService,
                                    MimeTypeRules.DefaultRules,
                                    previewCacheStorage);

                                using var searchIndexer = new LuceneIndexer();
                                var searchService = SearchServiceFactory.BuildSearchService(fileService, searchIndexer);

                                using var fileSystemCacheContext = new SqliteContext();
                                fileService.AddTestFileSystem(
                                    Url.Parse("file://local/"),
                                    new LocalFileSystemProvider(Path.GetFullPath("./Test")));

                                Server.Server.ConfigureAndRunWebHost(
                                    new Application(
                                        configuration,
                                        fileService,
                                        previewService,
                                        searchService));
                            }).Wait();
                    })
            };

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
