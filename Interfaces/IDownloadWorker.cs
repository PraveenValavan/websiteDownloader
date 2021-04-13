using System.Threading.Tasks;

namespace WebSiteDownloader
{
    public interface IDownloadWorker
    {
        public Task BeginDownloadAsync();
    }
}