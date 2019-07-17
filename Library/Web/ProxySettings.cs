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

            var splited = s.Split(PortDelimiter);

            if (splited.Length == 2 && ushort.TryParse(splited[1], out var port))
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

        public string Hostname { get; set; } = null;
        public ushort Port { get; set; } = 0;

        public ProxySettings()
        {

        }

        public ProxySettings(string hostname, ushort port)
        {
            this.Hostname = hostname;
            this.Port = port;
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
            var jObj = new JObject();
            jObj["Hostname"] = this.Hostname;
            jObj["Port"] = this.Port;

            return jObj;
        }

        public void Deserialize(JToken jToken)
        {
            this.Hostname = jToken.Value<string>("Hostname");
            this.Port = jToken.Value<ushort>("Port");
        }

    }

}
