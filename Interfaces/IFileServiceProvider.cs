namespace WebSiteDownloader
{
    public interface IFileServiceProvider
    {
        public void CheckAndCleanUpPreviousDownload();

        public void saveHTML(string subPath,string HTMLPage);

    }
}