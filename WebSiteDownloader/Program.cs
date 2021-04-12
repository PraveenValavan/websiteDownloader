using System;

namespace WebSiteDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            DownloadWorker downloadWorker = new DownloadWorker();
            downloadWorker.BeginDownload();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"elapsedMs : {elapsedMs}");
            Console.ReadLine();
        }
    }
}
