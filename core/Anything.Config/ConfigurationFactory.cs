using Microsoft.Extensions.Configuration;

namespace Anything.Config
{
    public static class ConfigurationFactory
    {
        public static IConfiguration BuildDevelopmentConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonString("{\"environment\": \"Development\"}");
            return configBuilder.Build();
        }
    }
}
