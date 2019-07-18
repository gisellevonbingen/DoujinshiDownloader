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
        public string DownloadDirectory { get; set; } = "Downloads";
        public bool DownloadCompleteAutoRemove { get; set; } = false;
        public bool DownloadToArchive { get; set; } = false;

        public ContentSettings()
        {

        }

        public void Read(JToken jToken)
        {
            this.DownloadDirectory = jToken.Value<string>("DownloadDirectory") ?? this.DownloadDirectory;
            this.DownloadCompleteAutoRemove = jToken.Value<bool?>("DownloadCompleteAutoRemove") ?? this.DownloadCompleteAutoRemove;
            this.DownloadToArchive = jToken.Value<bool?>("DownloadToArchive") ?? this.DownloadToArchive;
        }

        public void Write(JToken jToken)
        {
            jToken["DownloadDirectory"] = this.DownloadDirectory;
            jToken["DownloadCompleteAutoRemove"] = this.DownloadCompleteAutoRemove;
            jToken["DownloadToArchive"] = this.DownloadToArchive;
        }

    }

}
