using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WebSiteDownloader
{
    public class DownloadWorker : IDownloadWorker
    {
        private readonly IFileServiceProvider _fileServiceProvider;
        private readonly string _url;
        private readonly string _pattern;
        
        public DownloadWorker(IFileServiceProvider fileServiceProvider, IConfigurationProvider configurationProvider)
        {
            _fileServiceProvider = fileServiceProvider;
            _url = configurationProvider.url;
            _pattern = configurationProvider.pattern;
        }

        private List<String> _list = new List<string>();
        
        public void BeginDownload()
        {
            _fileServiceProvider.CheckAndCleanUpPreviousDownload();
            
            GetUrls(_url);
        }

        private void GetUrls(string pageUrl)
        {
            string page = DownloadWebsiteAsync(pageUrl);
            MatchCollection listingMatches = Regex.Matches(page, _pattern);
            foreach (Match match in listingMatches)
            {
                if (FilterMathes(match))
                    continue;

                var subUrl = match.Groups[1].Value.ToString().Trim('/');

                Console.WriteLine(_url + subUrl);

                _list.Add(subUrl);

                _fileServiceProvider.saveHTML(subUrl, page);

                GetUrls(_url + subUrl);
            }
        }

        private string DownloadWebsiteAsync(string pageUrl)
        {
            WebClient client = new WebClient();
            string HTMLPage = client.DownloadString(pageUrl);
            return HTMLPage;
        }

        private bool FilterMathes(Match match)
        {
            return match.Groups[1].ToString().Contains("http") || match.Groups[1].ToString().Contains("javascript:")
                    || match.Groups[1].ToString().Contains("{") || match.Groups[1].ToString().Contains("mailto:")
                    || match.Groups[1].ToString().Contains("#") || match.Groups[1].ToString().Contains("tel:")
                    || _list.Any(existingUrl => existingUrl.Equals(match.Groups[1].ToString().Trim('/')));
        }
    }
}
