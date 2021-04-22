using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebSiteDownloader
{
    public class DownloadWorker : IDownloadWorker
    {
        private readonly IFileServiceProvider _fileServiceProvider;
        private readonly ILogger<DownloadWorker> _logger;
        private readonly string _url;
        private readonly string _pattern;
        private List<Task> _tasks = new List<Task>();
        private ConcurrentBag<string> _controlList = new ConcurrentBag<string>();

        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        public DownloadWorker(IFileServiceProvider fileServiceProvider, IConfigurationProvider configurationProvider, ILogger<DownloadWorker> logger)
        {
            _fileServiceProvider = fileServiceProvider;
            _logger = logger;
            _url = configurationProvider.url;
            _pattern = configurationProvider.pattern;
        }

        public async Task BeginDownloadAsync()
        {
            Console.WriteLine($"Begining Download");

            _fileServiceProvider.CheckAndCleanUpPreviousDownload();

            IProgress<int> progress = new Progress<int>(progress =>
            {
                Console.Write($"\r Pages Downloaded : {progress}");
            });

            await GetUrlsAsync(_url, progress);

            Task.WhenAll(_tasks).Await(OnComplete, ErrorHandler);
        }
        
        private async Task GetUrlsAsync(string pageUrl, IProgress<int> progress)
        {

            string page = await DownloadWebsiteAsync(pageUrl);

            MatchCollection listingMatches = Regex.Matches(page, _pattern);

            foreach (Match match in listingMatches.ToList())
            {
                if (FilterMatches(match))
                    continue;

                var subUrl = match.Groups[1].Value.ToString().Trim('/');

                _controlList.Add(subUrl);

                progress?.Report(_controlList.Count);

                _fileServiceProvider.saveHTML(subUrl, page);

                _tasks.Add(GetUrlsAsync(_url + subUrl, progress));
            }
        }

        private async Task<string> DownloadWebsiteAsync(string pageUrl)
        {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(delegate (object sender, DownloadProgressChangedEventArgs e)
            {
                //Console.WriteLine("Downloaded:" + e.ProgressPercentage.ToString());
            });

            client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler
                (delegate (object sender, System.ComponentModel.AsyncCompletedEventArgs e)
                {
                    if (e.Error == null && !e.Cancelled)
                    {
                        //Console.WriteLine("Download completed!");
                    }
                });
            string HTMLPage = await client.DownloadStringTaskAsync(pageUrl);
            return HTMLPage;
        }

        private bool FilterMatches(Match match)
        {

            bool isMatchDownloadedOrNotOkay = false;
            isMatchDownloadedOrNotOkay = match.Groups[1].ToString().Contains("http") || match.Groups[1].ToString().Contains("javascript:")
                        || match.Groups[1].ToString().Contains("{") || match.Groups[1].ToString().Contains("mailto:")
                        || match.Groups[1].ToString().Contains("#") || match.Groups[1].ToString().Contains("tel:");

            if (_controlList.Count > 0)
                isMatchDownloadedOrNotOkay = isMatchDownloadedOrNotOkay || _controlList.Where(existingUrl => existingUrl.Equals(match.Groups[1].ToString().Trim('/'))).Any();

            return isMatchDownloadedOrNotOkay;
        }

        private void ErrorHandler(Exception ex, Task task)
        {
            _logger.LogError("Exception : {ExceptionString} {Task}", ex.ToString(), task.ToString());
        }
        private void OnComplete()
        {
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"\nDownload complete in  { elapsedMs / 1000 } seconds");
        }
    }
    public static class TaskExtentions
    {
        public async static void Await(this Task task, Action onComplete, Action<Exception, Task> errorHandler)
        {
            try
            {
                await task;
                onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex, task);
            }
        }
        public async static void Await(this Task task, Action<Exception> errorHandler)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex);
            }
        }
    }
}
