using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class AgentSettings : IJsonObject
    {
        public ExHentaiAccount ExHentaiAccount { get; set; } = null;

        public AgentSettings()
        {
            this.ExHentaiAccount = new ExHentaiAccount();
        }

        public void Read(JToken json)
        {
            this.ExHentaiAccount = json.Read<ExHentaiAccount>("ExHentaiAccount");
        }

        public void Write(JToken json)
        {
            json["ExHentaiAccount"] = this.ExHentaiAccount.Write();
        }

    }

}
