using Giselle.DoujinshiDownloader.Doujinshi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class AgentSettings
    {
        public ExHentaiAccount ExHentaiAccount { get; set; } = null;

        public AgentSettings()
        {
            this.ExHentaiAccount = new ExHentaiAccount();
        }

        public void Read(JToken jToken)
        {
            this.ExHentaiAccount.Deserialize(jToken.Value<JObject>("ExHentaiAccount") ?? new JObject());
        }

        public void Write(JToken jToken)
        {
            jToken["ExHentaiAccount"] = this.ExHentaiAccount.Serialize();
        }

    }

}
