using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OwnHub.File;
using OwnHub.File.Local;
using OwnHub.Preview.Icons;
using OwnHub.Preview.Metadata;
using OwnHub.Server.Api;
using OwnHub.Server.Api.Graphql.Schemas;
using OwnHub.Utils;

namespace OwnHub.Server
{
    public static class Server
    {
        public static void ConfigureAndRunWebHost()
        {
            int workerThreads;
            int portThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            Console.WriteLine("\nMaximum worker threads: \t{0}" +
                              "\nMaximum completion port threads: {1}",
                workerThreads, portThreads);

            ThreadPool.GetAvailableThreads(out workerThreads, 
                out portThreads);
            Console.WriteLine("\nAvailable worker threads: \t{0}" +
                              "\nAvailable completion port threads: {1}\n",
                workerThreads, portThreads);
            
            ThreadPool.GetMinThreads(out workerThreads, 
                out portThreads);
            Console.WriteLine("\nMinimum worker threads: \t{0}" +
                              "\nMinimum completion port threads: {1}\n",
                workerThreads, portThreads);
            
            ConfigureWebHostBuilder().Build().Run();
        }

        private static IWebHostBuilder ConfigureWebHostBuilder()
        {
            const string environment = "Development";

            return new WebHostBuilder()
                .UseKestrel()
                .UseEnvironment("Development")
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting()
                        .AddSingleton<MainSchema>()
                        .AddSingleton(MimeTypeRules.DefaultRules)
                        .AddSingleton((sp) => new StaticIconsService(Path.Join(Utils.Utils.GetApplicationRoot(), "/static-icons-cache.db")))
                        .AddSingleton((sp) => new DynamicIconsService(Path.Join(Utils.Utils.GetApplicationRoot(), "/dynamic-icons-cache.db")))
                        .AddSingleton((sp) => new MetadataService(Path.Join(Utils.Utils.GetApplicationRoot(), "/metadata-cache.db"), sp.GetRequiredService<ILogger<MetadataService>>()))
                        .AddSingleton<IFileSystem>(FileSystem.TestFilesystem)
                        .AddGraphQlService();

                    services.AddControllers();
                })
                .Configure(app =>
                {
                    if (environment == "Development") app.UseDeveloperExceptionPage();

                    app.UseRouting()
                        .UseGraphQL<MainSchema>("/api/graphql");

                    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

                    if (environment == "Development")
                        app
                            // Add the GraphQL Playground UI to try out the GraphQL API at /.
                            .UseGraphQLPlayground(new GraphQLPlaygroundOptions
                                {Path = "/", GraphQLEndPoint = "/api/graphql"})
                            // Add the GraphQL Voyager UI to let you navigate your GraphQL API as a spider graph at /voyager.
                            .UseGraphQLVoyager(new GraphQLVoyagerOptions
                                {Path = "/voyager", GraphQLEndPoint = "/api/graphql"});
                });
        }
    }
}