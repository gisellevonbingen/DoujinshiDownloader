using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class Configuration : IJsonObject
    {
        public ProgramSettings Program { get; } = new ProgramSettings();
        public AgentSettings Agent { get; } = new AgentSettings();
        public NetworkSettings Network { get; } = new NetworkSettings();
        public ContentSettings Content { get; } = new ContentSettings();
        public HookSettings Hook { get; } = new HookSettings();

        public Configuration()
        {

        }

        public void Read(JToken json)
        {
            var version = json.Value<int>("Version");

            if (version == 0)
            {
                this.Agent.ExHentaiAccount = json.Read<ExHentaiAccount>("Account");

                this.Network.Timeout = json.Value<int?>("Timeout") ?? 60 * 1000;
                this.Network.ThreadCount = json.Value<int?>("ThreadCount") ?? 4;
                this.Network.RetryCount = json.Value<int?>("RetryCount") ?? 2;

                this.Content.DownloadDirectory = json.Value<string>("DownloadDirectory") ?? "Downloads";
                this.Content.DownloadCompleteAutoRemove = json.Value<bool?>("DownloadCompleteAutoRemove") ?? false;
                this.Content.DownloadToArchive = json.Value<bool?>("DownloadToArchive") ?? false;
            }
            else
            {
                this.Program.Read(json.Value<JObject>("Program") ?? new JObject());
                this.Agent.Read(json.Value<JObject>("Agent") ?? new JObject());
                this.Network.Read(json.Value<JObject>("Network") ?? new JObject());
                this.Content.Read(json.Value<JObject>("Content") ?? new JObject());
                this.Hook.Read(json.Value<JObject>("Hook") ?? new JObject());
            }

        }

        public void Write(JToken json)
        {
            json["Version"] = 1;

            this.Program.Write(json["Program"] = new JObject());
            this.Agent.Write(json["Agent"] = new JObject());
            this.Network.Write(json["Network"] = new JObject());
            this.Content.Write(json["Content"] = new JObject());
            this.Hook.Write(json["Hook"] = new JObject());
        }

    }

}
