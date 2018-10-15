using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class ExHentaiAccount
    {
        private string _MemberId = null;
        public string MemberId { get { return this._MemberId; } set { this._MemberId = value; } }

        private string _PassHash = null;
        public string PassHash { get { return this._PassHash; } set { this._PassHash = value; } }

        public ExHentaiAccount()
        {

        }

        public ExHentaiAccount(string memberId, string passHash)
        {
            this.MemberId = memberId;
            this.PassHash = passHash;
        }

        public ExHentaiAccount(JToken jToken)
        {
            this.Deserialize(jToken);
        }

        public JToken Serialize()
        {
            JObject jObj = new JObject();

            jObj["MemberId"] = this.MemberId;
            jObj["PassHash"] = this.PassHash;

            return jObj;
        }

        public void Deserialize(JToken jToken)
        {
            this.MemberId = jToken.Value<string>("MemberId");
            this.PassHash = jToken.Value<string>("PassHash");
        }

    }

}
