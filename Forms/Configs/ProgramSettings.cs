using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class ProgramSettings
    {
        public bool AllowBackground { get; set; } = true;
        public bool AllowNotifyMessage { get; set; } = true;

        public NotifyMessageRules NotifyMessageRules { get; } = new NotifyMessageRules();

        public ProgramSettings()
        {

        }

        public void Read(JToken jToken)
        {
            this.AllowBackground = jToken.Value<bool?>("AllowBackground") ?? this.AllowBackground;
            this.AllowNotifyMessage = jToken.Value<bool?>("AllowNotifyMessage") ?? this.AllowNotifyMessage;

            this.NotifyMessageRules.Read(jToken.Value<JObject>("NotifyMessageRules") ?? new JObject());
        }

        public void Write(JToken jToken)
        {
            jToken["AllowBackground"] = this.AllowBackground;
            jToken["AllowNotifyMessage"] = this.AllowNotifyMessage;

            this.NotifyMessageRules.Write(jToken["NotifyMessageRules"] = new JObject());
        }

    }

}
