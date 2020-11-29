using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class ExHentaiAccount : IJsonObject
    {
        public string MemberId { get; set; } = null;
        public string PassHash { get; set; } = null;

        public ExHentaiAccount()
        {

        }

        public ExHentaiAccount(string memberId, string passHash)
        {
            this.MemberId = memberId;
            this.PassHash = passHash;
        }

        public void Read(JToken token)
        {
            this.MemberId = token.Value<string>("MemberId");
            this.PassHash = token.Value<string>("PassHash");
        }

        public void Write(JToken token)
        {
            token["MemberId"] = this.MemberId;
            token["PassHash"] = this.PassHash;
        }

    }

}
