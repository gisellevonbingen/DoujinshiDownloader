using Giselle.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class NetworkSettings : IJsonObject
    {
        public int Timeout { get; set; } = 60 * 1000;
        public int ThreadCount { get; set; } = 4;
        public int RetryCount { get; set; } = 2;

        public NetworkSettings()
        {

        }

        public void Read(JToken json)
        {
            this.Timeout = json.Value<int?>("Timeout") ?? this.Timeout;
            this.ThreadCount = json.Value<int?>("ThreadCount") ?? this.ThreadCount;
            this.RetryCount = json.Value<int?>("RetryCount") ?? this.RetryCount;
        }

        public void Write(JToken json)
        {
            json["Timeout"] = this.Timeout;
            json["ThreadCount"] = this.ThreadCount;
            json["RetryCount"] = this.RetryCount;
        }

    }

}
