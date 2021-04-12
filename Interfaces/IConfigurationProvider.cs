namespace WebSiteDownloader
{
    public interface IConfigurationProvider
    {
        public string url { get; }
        public string pattern { get; }
        public string outputPath { get; }
    }
}