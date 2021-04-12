using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WebSiteDownloader
{
    class DownloadWorker
    {
        private readonly string _url = "https://tretton37.com/";
        private List<String> _list = new List<string>();
        private readonly string pattern = @"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""";
        private readonly string outputPath = @"C:\Users\Praveen\source\repos\TrettonWebSiteDownloader\Downloaded\";

        public void BeginDownload()
        {
            prepareForDownload();
            GetUrls(_url);
        }

        private void prepareForDownload()
        {
            bool exists = Directory.Exists(outputPath);
            if (exists)
                Directory.Delete(outputPath, recursive: true);
        }

        private void GetUrls(string pageUrl)
        {
            string page = DownloadWebsiteAsync(pageUrl);
            MatchCollection listingMatches = Regex.Matches(page, pattern);
            foreach (Match match in listingMatches)
            {
                if (match.Groups[1].ToString().Contains("http") || match.Groups[1].ToString().Contains("javascript:")
                    || match.Groups[1].ToString().Contains("{") || match.Groups[1].ToString().Contains("mailto:")
                    || match.Groups[1].ToString().Contains("#") || match.Groups[1].ToString().Contains("tel:")
                    || _list.Any(existingUrl => existingUrl.Equals(match.Groups[1].ToString().Trim('/'))))
                    continue;

                var subUrl = match.Groups[1].Value.ToString().Trim('/');

                Console.WriteLine(_url + subUrl);
                
                _list.Add(subUrl);
                var filename = Regex.Matches(subUrl, @"[^/]*$")[0].Value;
                
                createfolder(outputPath + subUrl);
                
                File.WriteAllText($"{outputPath}{subUrl}//{(filename.Length > 0 ? filename : "index.html")}", page);
                
                GetUrls(_url + subUrl);
            }
        }

        private string DownloadWebsiteAsync(string pageUrl)
        {
            WebClient client = new WebClient();
            string HTMLPage =  client.DownloadString(pageUrl);
            return HTMLPage;
        }

        private void createfolder(string subPath)
        {
            bool exists = Directory.Exists(subPath);

            if (!exists)
                Directory.CreateDirectory(subPath);
        }
    }
}
