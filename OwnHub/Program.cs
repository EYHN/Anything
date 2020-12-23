using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OwnHub.File.Fork;
using OwnHub.Preview.Icons;

namespace OwnHub
{

    public static class Program
    {

        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand()
            {
                Description = "My sample app",
                Handler = CommandHandler.Create(Server.Server.ConfigureAndRunWebHost)
            };

            var iconsBuildCommand = new Command("build-icons", "Scan the icon directory and build icon cache database.")
            {
                new Option<string>(
                    "--database",
                    () => Path.Join(Utils.Utils.GetApplicationRoot(), "/static-icons-cache.db"),
                    "Cache database path."),
                new Option<string>(
                    "--directory",
                    () => Path.Join(Utils.Utils.GetApplicationRoot(), "/Icons"),
                    "Icon directory path."),
            };
            iconsBuildCommand.Handler = CommandHandler.Create((string database, string directory) =>
            {
                StaticIconsService.BuildCache(database, directory).Wait();
            });
            
            rootCommand.Add(iconsBuildCommand);
            
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}