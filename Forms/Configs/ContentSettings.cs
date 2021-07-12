using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class ContentSettings : IJsonObject
    {
        public string DownloadDirectory { get; set; } = "Downloads";
        public bool DownloadCompleteAutoRemove { get; set; } = false;
        public bool DownloadToArchive { get; set; } = false;

        public ContentSettings()
        {

        }

        public void Read(JToken json)
        {
            this.DownloadDirectory = json.Value<string>("DownloadDirectory") ?? this.DownloadDirectory;
            this.DownloadCompleteAutoRemove = json.Value<bool?>("DownloadCompleteAutoRemove") ?? this.DownloadCompleteAutoRemove;
            this.DownloadToArchive = json.Value<bool?>("DownloadToArchive") ?? this.DownloadToArchive;
        }

        public void Write(JToken json)
        {
            json["DownloadDirectory"] = this.DownloadDirectory;
            json["DownloadCompleteAutoRemove"] = this.DownloadCompleteAutoRemove;
            json["DownloadToArchive"] = this.DownloadToArchive;
        }

    }

}
