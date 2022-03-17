using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class NetworkSettings : IJsonObject
    {
        public int Timeout { get; set; } = 60_000;
        public int ThreadCount { get; set; } = 4;
        public int RetryCount { get; set; } = 2;
        public int ServiceUnavailableSleep { get; set; } = 5_000;

        public NetworkSettings()
        {

        }

        public void Read(JToken json)
        {
            this.Timeout = json.Value<int?>("Timeout") ?? this.Timeout;
            this.ThreadCount = json.Value<int?>("ThreadCount") ?? this.ThreadCount;
            this.RetryCount = json.Value<int?>("RetryCount") ?? this.RetryCount;
            this.ServiceUnavailableSleep = json.Value<int?>("ServiceUnavailableSleep") ?? this.ServiceUnavailableSleep;
        }

        public void Write(JToken json)
        {
            json["Timeout"] = this.Timeout;
            json["ThreadCount"] = this.ThreadCount;
            json["RetryCount"] = this.RetryCount;
            json["ServiceUnavailableSleep"] = this.ServiceUnavailableSleep;
        }

    }

}
