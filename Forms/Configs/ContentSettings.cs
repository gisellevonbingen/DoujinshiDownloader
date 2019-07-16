using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class ContentSettings
    {
        public string DownloadDirectory { get; set; } = null;
        public bool DownloadCompleteAutoRemove { get; set; } = false;
        public bool DownloadToArchive { get; set; } = false;

        public ContentSettings()
        {

        }

        public void Read(JToken jToken)
        {
            this.DownloadDirectory = jToken.Value<string>("DownloadDirectory") ?? "Downloads";
            this.DownloadCompleteAutoRemove = jToken.Value<bool?>("DownloadCompleteAutoRemove") ?? false;
            this.DownloadToArchive = jToken.Value<bool?>("DownloadToArchive") ?? false;
        }

        public void Write(JToken jToken)
        {
            jToken["DownloadDirectory"] = this.DownloadDirectory;
            jToken["DownloadCompleteAutoRemove"] = this.DownloadCompleteAutoRemove;
            jToken["DownloadToArchive"] = this.DownloadToArchive;
        }

    }

}
