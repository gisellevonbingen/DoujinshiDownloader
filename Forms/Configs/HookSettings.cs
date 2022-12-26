using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Collections;
using Giselle.DoujinshiDownloader.Hooks;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class HookSettings : IJsonObject
    {
        public bool WaitForExit { get; set; } = false;
        public HookCommandlineSettings Commandlines { get; } = new HookCommandlineSettings();

        public HookSettings()
        {

        }

        public void Read(JToken json)
        {
            this.WaitForExit = json.Value<bool>("WaitForExit");
            this.Commandlines.Read(json.Value<JObject>("Commandlines") ?? new JObject());
        }

        public void Write(JToken json)
        {
            json["WaitForExit"] = this.WaitForExit;
            json["Commandlines"] = this.Commandlines.Write();
        }

        public class HookCommandlineSettings : IJsonObject
        {
            private readonly Dictionary<string, string> Map = new Dictionary<string, string>();

            public string this[Hook hook]
            {
                get
                {
                    return this.Map.GetSafe(hook.Name, string.Empty);
                }

                set
                {
                    if (string.IsNullOrEmpty(value) == true)
                    {
                        this.Map.Remove(hook.Name);
                    }
                    else
                    {
                        this.Map[hook.Name] = value;
                    }

                }

            }

            public void Read(JToken json)
            {
                this.Map.Clear();

                foreach (var hook in Hook.Registry.Values)
                {
                    this[hook] = json.Value<string>(hook.Name);
                }

            }

            public void Write(JToken json)
            {
                foreach (var hook in Hook.Registry.Values)
                {
                    json[hook.Name] = this[hook];
                }

            }

        }

    }

}
