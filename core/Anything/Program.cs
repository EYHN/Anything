using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Anything.Config;
using Anything.Database;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.FileSystem.Tracker.Database;
using Anything.Notes;
using Anything.Preview;
using Anything.Preview.Mime;
using Anything.Search;
using Anything.Search.Indexers;
using Anything.Server.Models;
using Anything.Tags;

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
                            () =>
                            {
                                var configuration = ConfigurationFactory.BuildDevelopmentConfiguration();

                                using var fileService = new FileService();

                                using var previewCacheStorage = new PreviewMemoryCacheStorage(fileService);
                                var previewService = new PreviewService(
                                    fileService,
                                    MimeTypeRules.DefaultRules,
                                    previewCacheStorage);

                                using var searchIndexer = new LuceneIndexer();
                                using var searchService = SearchServiceFactory.BuildSearchService(fileService, searchIndexer);

                                using var tagStorage = new TagService.MemoryStorage();
                                using var tagService = new TagService(fileService, tagStorage);

                                using var noteStorage = new NoteService.MemoryStorage();
                                using var noteService = new NoteService(fileService, noteStorage);

                                using var trackerStorage = new HintFileTracker.MemoryStorage();
                                using var localFileSystem = new LocalFileSystem(Path.GetFullPath("./Test"), trackerStorage);
                                using var fileSystemCacheContext = new SqliteContext();
                                fileService.AddFileSystem(
                                    "local",
                                    localFileSystem);

                                Server.Server.ConfigureAndRunWebHost(
                                    new Application(
                                        configuration,
                                        fileService,
                                        previewService,
                                        searchService,
                                        tagService,
                                        noteService));
                            }).Wait();
                    })
            };

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
