using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WebSiteDownloader
{
    class FileServiceProvider: IFileServiceProvider
    {
        private readonly IConfigurationProvider _configurationProvider;
        public FileServiceProvider(IConfigurationProvider configurationProvider) 
        {
            _configurationProvider = configurationProvider;
        }
        public void CheckAndCleanUpPreviousDownload()
        {
            bool exists = Directory.Exists(_configurationProvider.outputPath);
            if (exists)
                Directory.Delete(_configurationProvider.outputPath, recursive: true);
        }
        private void CreateFolder(string subPath)
        {
            bool exists = Directory.Exists(subPath);

            if (!exists)
                Directory.CreateDirectory(subPath);
        }

        public void saveHTML(string subPath,string HTMLPage)
        {
            string filename = Regex.Matches(subPath, @"[^/]*$")[0].Value;

            CreateFolder(_configurationProvider.outputPath + subPath);

            File.WriteAllText($"{_configurationProvider.outputPath}{subPath}//{(filename.Length > 0 ? filename : "index.html")}", HTMLPage);
        }
    }
}
