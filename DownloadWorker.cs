using System;
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
        private readonly string _url;
        private readonly string _pattern;

        private List<String> _list = new List<string>();
        private List<Task> tasks = new List<Task>();

        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        public DownloadWorker(IFileServiceProvider fileServiceProvider, IConfigurationProvider configurationProvider)
        {
            _fileServiceProvider = fileServiceProvider;
            _url = configurationProvider.url;
            _pattern = configurationProvider.pattern;
        }

        public async Task BeginDownload()
        {
            Console.WriteLine($"Begining Download");



            _fileServiceProvider.CheckAndCleanUpPreviousDownload();

            IProgress<int> progress = new Progress<int>(progress =>
            {
                Console.Write($"\r Pages Downloaded : {progress}");
            });

            await GetUrlsAsync(_url, progress);

            Task.WhenAll(tasks).Await(OnComplete, ErrorHandler);
        }

        private void OnComplete()
        {
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"\nDownload complete in  { elapsedMs / 1000 } seconds");
        }

        private async Task GetUrlsAsync(string pageUrl, IProgress<int> progress)
        {
            try
            {
                string page = await DownloadWebsiteAsync(pageUrl);

                MatchCollection listingMatches = Regex.Matches(page, _pattern);

                foreach (Match match in listingMatches.ToList())
                {
                    if (FilterMathes(match))
                        continue;

                    var subUrl = match.Groups[1].Value.ToString().Trim('/');

                    _list.Add(subUrl);

                    _fileServiceProvider.saveHTML(subUrl, page);

                    progress?.Report(_list.Count);

                    tasks.Add(GetUrlsAsync(_url + subUrl, progress));
                }
            }
            catch(Exception ex)
            {
                throw ex;
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

        private bool FilterMathes(Match match)
        {
            try
            {
                return match.Groups[1].ToString().Contains("http") || match.Groups[1].ToString().Contains("javascript:")
                        || match.Groups[1].ToString().Contains("{") || match.Groups[1].ToString().Contains("mailto:")
                        || match.Groups[1].ToString().Contains("#") || match.Groups[1].ToString().Contains("tel:")
                        || _list.ToList().Where(existingUrl => existingUrl.Equals(match.Groups[1].ToString().Trim('/'))).Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ErrorHandler(Exception ex,Task task)
        {
            throw ex;
        }
    }
    public static class TaskExtentions
    {
        public async static void Await(this Task task, Action onComplete, Action<Exception,Task> errorHandler)
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
