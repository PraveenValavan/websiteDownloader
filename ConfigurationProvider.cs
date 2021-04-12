using System;
using System.Collections.Generic;
using System.Text;

namespace WebSiteDownloader
{
    class ConfigurationProvider : IConfigurationProvider
    {
        public string url { get; } = "https://tretton37.com/";
        public string pattern { get; } = @"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""";
        public string outputPath { get; } = @".\Downloaded\";
    }
}
