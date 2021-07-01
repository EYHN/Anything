using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Anything.Config
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddJsonString(this IConfigurationBuilder configurationBuilder, string jsonString)
        {
            return configurationBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(jsonString)));
        }
    }
}
