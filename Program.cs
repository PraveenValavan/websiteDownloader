using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebSiteDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configurationBuilder.Build())
                .CreateLogger();

            var host = Host.CreateDefaultBuilder()
                            .UseSerilog()
                            .ConfigureServices((services) =>
                            {
                                services.AddTransient<IDownloadWorker, DownloadWorker>();
                                services.AddTransient<IFileServiceProvider, FileServiceProvider>();
                                services.AddTransient<IConfigurationProvider, ConfigurationProvider>();
                            })
                           .Build();

            Log.Information("Starting {ApplicationName} ", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

            try
            {
                var service = ActivatorUtilities.CreateInstance<DownloadWorker>(host.Services);
                await service.BeginDownloadAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal("\nException : {ExceptionString}", ex.ToString());
            }
            finally
            {
                Log.CloseAndFlush();
            }
            Console.ReadLine();
        }
    }
}
