using System;
using System.Threading;
using Anything.Server.Api.Graphql.Schemas;
using Anything.Server.Models;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Anything.Server.Api;

namespace Anything.Server
{
    public static class Server
    {
        public static void ConfigureAndRunWebHost(Application application)
        {
            int workerThreads;
            int portThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
            Console.WriteLine(
                "\nMaximum worker threads: \t{0}" +
                "\nMaximum completion port threads: {1}",
                workerThreads,
                portThreads);

            ThreadPool.GetAvailableThreads(
                out workerThreads,
                out portThreads);
            Console.WriteLine(
                "\nAvailable worker threads: \t{0}" +
                "\nAvailable completion port threads: {1}\n",
                workerThreads,
                portThreads);

            ThreadPool.GetMinThreads(
                out workerThreads,
                out portThreads);
            Console.WriteLine(
                "\nMinimum worker threads: \t{0}" +
                "\nMinimum completion port threads: {1}\n",
                workerThreads,
                portThreads);

            ConfigureWebHostBuilder(application).Build().Run();
        }

        private static IWebHostBuilder ConfigureWebHostBuilder(Application application)
        {
            const string environment = "Development";

            return new WebHostBuilder()
                .UseKestrel()
                .UseEnvironment("Development")
                .ConfigureLogging(
                    logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    })
                .ConfigureServices(
                    services =>
                    {
                        services
                            .AddRouting()
                            .AddSingleton(application)
                            .AddSingleton(new MainSchema(application))
                            .AddGraphQlService();

                        services.AddControllers();
                    })
                .Configure(
                    app =>
                    {
                        if (environment == "Development")
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.UseRouting()
                            .UseGraphQL<MainSchema>("/api/graphql");

                        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

                        if (environment == "Development")
                        {
                            // Add the GraphQL Playground UI to try out the GraphQL API at /.
                            app.UseGraphQLPlayground(new PlaygroundOptions { GraphQLEndPoint = "/api/graphql" }, "/");

                            // Add the GraphQL Voyager UI to let you navigate your GraphQL API as a spider graph at /voyager.
                            app.UseGraphQLVoyager(new VoyagerOptions { GraphQLEndPoint = "/api/graphql" }, "/voyager");
                        }
                    });
        }
    }
}
