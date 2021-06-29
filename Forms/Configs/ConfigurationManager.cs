using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class ConfigurationManager
    {
        public string Path { get; }
        public Configuration Values { get; }

        public event EventHandler Loaded;
        public event EventHandler Saved;

        public ConfigurationManager(string path)
        {
            this.Path = path;
            this.Values = new Configuration();
        }

        protected virtual void OnLoaded(EventArgs e)
        {
            this.Loaded?.Invoke(this, e);
        }

        protected virtual void OnSaved(EventArgs e)
        {
            this.Saved?.Invoke(this, e);
        }

        public void Load()
        {
            var jToken = this.LoadJson();

            var values = this.Values;
            values.Read(jToken);

            this.OnLoaded(EventArgs.Empty);

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

            this.OnSaved(EventArgs.Empty);
        }

    }

}
