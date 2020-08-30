using OwnHub.File;
using OwnHub.File.Local;
using OwnHub.Preview.Icons;
using OwnHub.Server.Api;
using OwnHub.Server.Api.Graphql.Schemas;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace OwnHub.Server
{
    public class Server
    {
        public static void ConfigureAndRunWebHost()
        {
            ConfigureWebHostBuilder().Build().Run();
        }

        public static IWebHostBuilder ConfigureWebHostBuilder()
        {
            const string Environment = "Development";


            var StaticIconsCacheDatabase = new IconsDatabase(Path.Join(Utils.Utils.GetApplicationRoot(), "/iconcache.db"), Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly);
            StaticIconsCacheDatabase.Open().Wait();

            var DynamicIcons = new DynamicIconsService();
            var DynamicIconsCacheDatabase = new IconsDatabase(Path.Join(Utils.Utils.GetApplicationRoot(), "/dynamic-icons-cache.db"), Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate);
            DynamicIconsCacheDatabase.Open().Wait();
            DynamicIcons.CacheDatabase = DynamicIconsCacheDatabase;

            return new WebHostBuilder()
                .UseKestrel()
                .UseEnvironment("Development")
                .ConfigureServices(services =>
                {
                    services
                    .AddRouting()
                    .AddSingleton<MainSchema>()
                    .AddSingleton<MimeTypeRules>(MimeTypeRules.DefaultRules)
                    .AddSingleton<IconsDatabase>(StaticIconsCacheDatabase)
                    .AddSingleton<DynamicIconsService>(DynamicIcons)
                    .AddSingleton<IFileSystem>(FileSystem._test_filesystem)
                    .AddGraphQLService();

                    services.AddControllers();
                })
                .Configure(app =>
                {
                    if (Environment == "Development")
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    app.UseRouting()
                        .UseGraphQL<MainSchema>(path: "/api/graphql");

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });

                    if (Environment == "Development")
                    {
                        app
                            // Add the GraphQL Playground UI to try out the GraphQL API at /.
                            .UseGraphQLPlayground(new GraphQLPlaygroundOptions() { Path = "/", GraphQLEndPoint = "/api/graphql" })
                            // Add the GraphQL Voyager UI to let you navigate your GraphQL API as a spider graph at /voyager.
                            .UseGraphQLVoyager(new GraphQLVoyagerOptions() { Path = "/voyager", GraphQLEndPoint = "/api/graphql" });
                    }
                });
        }
    }
}
