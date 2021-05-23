using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Provider;
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
                                var fileSystemService = new VirtualFileSystemService();
                                fileSystemService.RegisterFileSystemProvider("memory", new MemoryFileSystemProvider());
                                var previewService = await PreviewServiceFactory.BuildPreviewService(
                                    fileSystemService,
                                    MimeTypeRules.TestRules,
                                    Path.GetFullPath("./cache"));
                                Server.Server.ConfigureAndRunWebHost(new Application(fileSystemService, previewService));
                            }).Wait();
                    })
            };

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
