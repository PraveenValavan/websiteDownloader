using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebSiteDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                            .ConfigureServices((context, services) =>
                            {
                                services.AddTransient<IDownloadWorker, DownloadWorker>();
                                services.AddTransient<IFileServiceProvider, FileServiceProvider>();
                                services.AddTransient<IConfigurationProvider, ConfigurationProvider>();
                            })
                           .Build();

            var service = ActivatorUtilities.CreateInstance<DownloadWorker>(host.Services);

            service.BeginDownload();
            Console.ReadLine();
        }
    }
}
