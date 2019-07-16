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
    public class Configuration
    {
        public ExHentaiAccount Account { get; set; } = null;
        public int Timeout { get; set; } = 0;
        public int ThreadCount { get; set; } = 0;
        public int RetryCount { get; set; } = 0;
        public string DownloadDirectory { get; set; } = null;
        public bool DownloadCompleteAutoRemove { get; set; } = false;
        public bool DownloadToArchive { get; set; } = false;

        public Configuration()
        {
            this.Account = new ExHentaiAccount();
        }

        public void Read(JToken jToken)
        {
            this.Account.Deserialize(jToken.Value<JObject>("Account") ?? new JObject());
            this.Timeout = jToken.Value<int?>("Timeout") ?? 60 * 1000;
            this.ThreadCount = jToken.Value<int?>("ThreadCount") ?? 4;
            this.RetryCount = jToken.Value<int?>("RetryCount") ?? 2;

            this.DownloadDirectory = jToken.Value<string>("DownloadDirectory") ?? "Downloads";
            this.DownloadCompleteAutoRemove = jToken.Value<bool?>("DownloadCompleteAutoRemove") ?? false;
            this.DownloadToArchive = jToken.Value<bool?>("DownloadToArchive") ?? false;
        }

        public void Write(JToken jToken)
        {
            jToken["Account"] = this.Account.Serialize();
            jToken["Timeout"] = this.Timeout;
            jToken["ThreadCount"] = this.ThreadCount;
            jToken["RetryCount"] = this.RetryCount;

            jToken["DownloadDirectory"] = this.DownloadDirectory;
            jToken["DownloadCompleteAutoRemove"] = this.DownloadCompleteAutoRemove;
            jToken["DownloadToArchive"] = this.DownloadToArchive;
        }

    }

}
