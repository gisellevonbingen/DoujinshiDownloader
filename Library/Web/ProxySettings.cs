using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Web
{
    public class ProxySettings
    {
        public const char PortDelimiter = ':';

        public static bool TryParse(string s, out ProxySettings settings)
        {
            if (s == null)
            {
                settings = null;
                return false;
            }

            string[] splited = s.Split(PortDelimiter);
            ushort port = 0;

            if (splited.Length == 2 && ushort.TryParse(splited[1], out port))
            {
                settings = new ProxySettings(splited[0], port);
                return true;
            }
            else
            {
                settings = null;
                return false;
            }

        }

        private string _Hostname = null;
        public string Hostname { get { return this._Hostname; } set { this._Hostname = value; } }

        private ushort _Port = 0;
        public ushort Port { get { return this._Port; } set { this._Port = value; } }

        public ProxySettings()
        {

        }

        public ProxySettings(string hostname, ushort port)
        {
            this._Hostname = hostname;
            this._Port = port;
        }

        public ProxySettings(JToken jToken)
        {
            this.Deserialize(jToken);
        }

        public override string ToString()
        {
            return this.Hostname + PortDelimiter + this.Port;
        }

        public JToken Serialize()
        {
            JObject jObj = new JObject();

            jObj["Hostname"] = this.Hostname;
            jObj["Port"] = this.Port;

            return jObj;
        }

        public void Deserialize(JToken jToken)
        {
            this.Hostname = jToken["Hostname"].Value<string>();
            this.Port = jToken["Port"].Value<ushort>();
        }

    }

}
