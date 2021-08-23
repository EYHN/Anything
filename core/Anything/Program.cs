using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Anything.Config;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
using Anything.Preview;
using Anything.Preview.Mime;
using Anything.Search;
using Anything.Server.Models;
using Anything.Utils;
using Directory = System.IO.Directory;

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

                                var cachePath = Path.GetFullPath(Environment.GetEnvironmentVariable("ANYTHING_CACHE_PATH") ?? "./cache");

                                Directory.CreateDirectory(cachePath);

                                var fileService = new FileService();
                                fileService.AddTestFileSystem(
                                    Url.Parse("file://local/"),
                                    new LocalFileSystemProvider(Path.GetFullPath("./Test")),
                                    Path.Join(cachePath, "tracker.db"));

                                var previewService = await PreviewServiceFactory.BuildPreviewService(
                                    fileService,
                                    MimeTypeRules.DefaultRules,
                                    cachePath);
                                var searchService = SearchServiceFactory.BuildSearchService(fileService, Path.Join(cachePath, "index"));
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
