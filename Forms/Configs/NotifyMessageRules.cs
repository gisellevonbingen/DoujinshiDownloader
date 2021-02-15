using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class NotifyMessageRules : IJsonObject
    {
        public bool HideMainForm { get; set; } = true;
        public bool DownlaodComplete { get; set; } = true;

        public NotifyMessageRules()
        {

        }

        public void Read(JToken json)
        {
            this.HideMainForm = json.Value<bool?>("HideMainForm") ?? this.HideMainForm;
            this.DownlaodComplete = json.Value<bool?>("DownlaodComplete") ?? this.DownlaodComplete;
        }

        public void Write(JToken json)
        {
            json["HideMainForm"] = this.HideMainForm;
            json["DownlaodComplete"] = this.DownlaodComplete;
        }

    }

}
