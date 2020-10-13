using OwnHub.Preview.Icons;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System;
using System.IO;
using OwnHub.Preview.Icons.Renderers;
using OwnHub.File.Virtual;
using OwnHub.File;
using System.Text;
using OwnHub.Preview;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using System.Reactive.Linq;
using System.Security.Cryptography;

namespace OwnHub
{
    public class Program
    {
        public static string MD5(string content)
        {
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(content));
            string str = "";
            for (int index = 0; index < hash.Length; ++index)
                str += hash[index].ToString("x").PadLeft(2, '0');
            return str;
        }

        static int Main(string[] args)
        {
            var rootCommand = new RootCommand();

            rootCommand.Description = "My sample app";

            rootCommand.Handler = CommandHandler.Create(() =>
            {
                Server.Server.ConfigureAndRunWebHost();
            });

            var iconsBuildCommand = new Command("build-icons", "Scan the icon directory and build icon cache database.") {
                new Option<string>(
                    "--database",
                    getDefaultValue: () => Path.Join(Utils.Utils.GetApplicationRoot(), "/iconcache.db"),
                    description: "Cache database path."),
                new Option<string>(
                    "--directory",
                    getDefaultValue: () => Path.Join(Utils.Utils.GetApplicationRoot(), "/Icons"),
                    description: "Icon directory path.")
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