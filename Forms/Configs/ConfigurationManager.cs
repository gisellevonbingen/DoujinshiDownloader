using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class ConfigurationManager
    {
        public string Path { get; }
        public Configuration Values { get; }

        public ConfigurationManager(string path)
        {
            this.Path = path;
            this.Values = new Configuration();
        }

        public void Load()
        {
            var jToken = this.LoadJson();

            var values = this.Values;
            values.Read(jToken);

            this.Save();
        }

        private JToken LoadJson()
        {
            if (File.Exists(this.Path) == true)
            {
                var json = File.ReadAllText(this.Path);
                return JObject.Parse(json);
            }
            else
            {
                return new JObject();
            }

        }

        public void Save()
        {
            var jToken = new JObject();
            this.Values.Write(jToken);

            File.WriteAllText(this.Path, jToken.ToString(Newtonsoft.Json.Formatting.Indented));
        }

    }

}
