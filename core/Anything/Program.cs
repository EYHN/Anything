using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Preview;
using Anything.Preview.MimeType;
using Anything.Server.Models;

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
                                var cachePath = Path.GetFullPath(Environment.GetEnvironmentVariable("ANYTHING_CACHE_PATH") ?? "./cache");
                                var fileService = await FileServiceFactory.BuildLocalFileService(Path.GetFullPath("./Test"), cachePath);
                                var previewService = await PreviewServiceFactory.BuildPreviewService(
                                    fileService,
                                    MimeTypeRules.DefaultRules,
                                    cachePath);
                                Server.Server.ConfigureAndRunWebHost(new Application(fileService, previewService));
                            }).Wait();
                    })
            };

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
