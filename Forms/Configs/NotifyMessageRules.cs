using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class NotifyMessageRules
    {
        public bool HideMainForm { get; set; } = true;
        public bool DownlaodComplete { get; set; } = true;

        public NotifyMessageRules()
        {

        }

        public void Read(JToken jToken)
        {
            this.HideMainForm = jToken.Value<bool?>("HideMainForm") ?? true;
            this.DownlaodComplete = jToken.Value<bool?>("DownlaodComplete") ?? true;
        }

        public void Write(JToken jToken)
        {
            jToken["HideMainForm"] = this.HideMainForm;
            jToken["DownlaodComplete"] = this.DownlaodComplete;
        }

    }

}
