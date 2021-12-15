using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Anything;

public static class Program
{
    private static int Main(string[] args)
    {
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(ConfigureServerCommand());

        return rootCommand.InvokeAsync(args).Result;
    }

    private static Command ConfigureServerCommand()
    {
        var serverCommand =
            new Command("server") { new Option<bool>("--print-graphql-schema", () => false, "Print the GraphQL schema and exit") };

        serverCommand.Handler = CommandHandler.Create<bool>(printGraphQlSchema =>
        {
            using var webApp = Server.Server.ConfigureWebApplication(builder =>
            {
                Bootstrap.ConfigureFeature(builder.Services);
                Bootstrap.ConfigureFileService(builder.Services);
                Bootstrap.ConfigureLogging(builder.Logging);
            });

            if (printGraphQlSchema)
            {
                var schema = webApp.Services.GetRequiredService<ISchema>();
                var printer = new SchemaPrinter(schema);
                Console.WriteLine(printer.Print());
                return;
            }

            webApp.Run();
        });

        return serverCommand;
    }
}
