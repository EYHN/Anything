using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Anything.Config
{
    public static class ConfigurationFactory
    {
        public static IConfiguration BuildDevelopmentConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes("{\"environment\": \"Development\"}"));
            configBuilder.AddJsonStream(jsonStream);
            return configBuilder.Build();
        }
    }
}
