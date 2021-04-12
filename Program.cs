using SimpleInjector;
using System;

namespace WebSiteDownloader
{
    class Program
    {
        static void Main(string[] args)
        {

            Container _container = new Container();
            _container.Register<IDownloadWorker, DownloadWorker>();
            _container.Register<IFileServiceProvider, FileServiceProvider>();
            _container.Register<IConfigurationProvider, ConfigurationProvider>();

            _container.Verify();
            var _downloadWorker = _container.GetInstance<IDownloadWorker>();

            _downloadWorker.BeginDownload();

            Console.ReadLine();
        }
    }
}
