using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class NumberingSettings : IJsonObject
    {
        public bool Enabled { get; set; } = false;
        public bool AutoDigits { get; set; } = true;
        public int Digits { get; set; } = 0;

        public NumberingSettings()
        {

        }

        public void Read(JToken json)
        {
            this.Enabled = json.Value<bool>("Enabled");
            this.AutoDigits = json.Value<bool>("AutoDigits");
            this.Digits = json.Value<int>("Digits");
        }

        public void Write(JToken json)
        {
            json["Enabled"] = this.Enabled;
            json["AutoDigits"] = this.AutoDigits;
            json["Digits"] = this.Digits;
        }

    }

}
