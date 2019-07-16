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
        public int Timeout { get; set; } = 0;
        public int ThreadCount { get; set; } = 0;
        public int RetryCount { get; set; } = 0;

        public NetworkSettings()
        {

        }

        public void Read(JToken jToken)
        {
            this.Timeout = jToken.Value<int?>("Timeout") ?? 60 * 1000;
            this.ThreadCount = jToken.Value<int?>("ThreadCount") ?? 4;
            this.RetryCount = jToken.Value<int?>("RetryCount") ?? 2;
        }

        public void Write(JToken jToken)
        {
            jToken["Timeout"] = this.Timeout;
            jToken["ThreadCount"] = this.ThreadCount;
            jToken["RetryCount"] = this.RetryCount;
        }

    }

}
