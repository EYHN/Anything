using System;
using Anything.Server.Api.Graphql.Schemas;
using GraphQL.Utilities;

namespace Anything.Tools.GraphqlSchema
{
    public static class Program
    {
        public static void Main()
        {
            using var schema = new MainSchema();

            var printer = new SchemaPrinter(schema);

            Console.WriteLine(printer.Print());
        }
    }
}
