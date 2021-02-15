using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class ProgramSettings : IJsonObject
    {
        public bool AllowBackground { get; set; } = true;
        public bool AllowNotifyMessage { get; set; } = true;

        public NotifyMessageRules NotifyMessageRules { get; } = new NotifyMessageRules();
        public UserInterfaceRules UserInterfaceRules { get; } = new UserInterfaceRules();

        public ProgramSettings()
        {

        }

        public void Read(JToken json)
        {
            this.AllowBackground = json.Value<bool?>("AllowBackground") ?? this.AllowBackground;
            this.AllowNotifyMessage = json.Value<bool?>("AllowNotifyMessage") ?? this.AllowNotifyMessage;

            this.NotifyMessageRules.Read(json.Value<JObject>("NotifyMessageRules") ?? new JObject());
            this.UserInterfaceRules.Read(json.Value<JObject>("UserInterfaceRules") ?? new JObject());
        }

        public void Write(JToken json)
        {
            json["AllowBackground"] = this.AllowBackground;
            json["AllowNotifyMessage"] = this.AllowNotifyMessage;

            this.NotifyMessageRules.Write(json["NotifyMessageRules"] = new JObject());
            this.UserInterfaceRules.Write(json["UserInterfaceRules"] = new JObject());
        }

    }

}
