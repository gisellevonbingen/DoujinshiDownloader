using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader
{
    public class Settings
    {
        public string Path { get; }

        public ExHentaiAccount Account { get; set; } = null;
        public int Timeout { get; set; } = 0;
        public int ThreadCount { get; set; } = 0;
        public int RetryCount { get; set; } = 0;
        public string DownloadDirectory { get; set; } = null;
        public bool DownloadCompleteAutoRemove { get; set; } = false;

        public Settings(string path)
        {
            this.Path = path;
        }

        public void Load()
        {
            var jObject = this.Read();

            this.Account = new ExHentaiAccount(jObject["Account"] ?? new JObject());
            this.Timeout = ((int?)jObject["Timeout"]).GetValueOrDefault(60 * 1000);
            this.ThreadCount = ((int?)jObject["ThreadCount"]).GetValueOrDefault(4);
            this.RetryCount = ((int?)jObject["RetryCount"]).GetValueOrDefault(2);

            this.DownloadDirectory = ((string)jObject["DownloadDirectory"]) ?? "Downloads";
            this.DownloadCompleteAutoRemove = ((bool?)jObject["DownloadCompleteAutoRemove"]).GetValueOrDefault(false);

            this.Save();
        }

        public void Save()
        {
            var jObject = new JObject();

            jObject["Account"] = this.Account.Serialize();
            jObject["Timeout"] = this.Timeout;
            jObject["ThreadCount"] = this.ThreadCount;
            jObject["RetryCount"] = this.RetryCount;
            jObject["DownloadDirectory"] = this.DownloadDirectory;
            jObject["DownloadCompleteAutoRemove"] = this.DownloadCompleteAutoRemove;

            this.Write(jObject);
        }

        private JObject Read()
        {
            string path = this.Path;

            if (File.Exists(path) == true)
            {
                return JObject.Parse(File.ReadAllText(path));
            }
            else
            {
                return new JObject();
            }

        }

        private void Write(JObject jObject)
        {
            File.WriteAllText(this.Path, jObject.ToString(Newtonsoft.Json.Formatting.None));
        }

    }

}
