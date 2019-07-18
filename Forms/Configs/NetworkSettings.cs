using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class NetworkSettings
    {
        public int Timeout { get; set; } = 60 * 1000;
        public int ThreadCount { get; set; } = 4;
        public int RetryCount { get; set; } = 2;

        public NetworkSettings()
        {

        }

        public void Read(JToken jToken)
        {
            this.Timeout = jToken.Value<int?>("Timeout") ?? this.Timeout;
            this.ThreadCount = jToken.Value<int?>("ThreadCount") ?? this.ThreadCount;
            this.RetryCount = jToken.Value<int?>("RetryCount") ?? this.RetryCount;
        }

        public void Write(JToken jToken)
        {
            jToken["Timeout"] = this.Timeout;
            jToken["ThreadCount"] = this.ThreadCount;
            jToken["RetryCount"] = this.RetryCount;
        }

    }

}
