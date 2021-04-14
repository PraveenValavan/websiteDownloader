using Microsoft.Extensions.Configuration;

namespace WebSiteDownloader
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfiguration _configuration;
        public ConfigurationProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            url = _configuration.GetValue<string>("url");
            outputPath = _configuration.GetValue<string>("outputPath");
        }

        public string url { get; }
        public string pattern { get; } = @"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""";
        public string outputPath { get; }

    }
}
