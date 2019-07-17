using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class Configuration
    {
        public ProgramSettings Program { get; } = new ProgramSettings();
        public AgentSettings Agent { get; } = new AgentSettings();
        public NetworkSettings Network { get; } = new NetworkSettings();
        public ContentSettings Content { get; } = new ContentSettings();

        public Configuration()
        {

        }

        public void Read(JToken jToken)
        {
            var version = jToken.Value<int>("Version");

            if (version == 0)
            {
                this.Agent.ExHentaiAccount.Deserialize(jToken.Value<JObject>("Account") ?? new JObject());

                this.Network.Timeout = jToken.Value<int?>("Timeout") ?? 60 * 1000;
                this.Network.ThreadCount = jToken.Value<int?>("ThreadCount") ?? 4;
                this.Network.RetryCount = jToken.Value<int?>("RetryCount") ?? 2;

                this.Content.DownloadDirectory = jToken.Value<string>("DownloadDirectory") ?? "Downloads";
                this.Content.DownloadCompleteAutoRemove = jToken.Value<bool?>("DownloadCompleteAutoRemove") ?? false;
                this.Content.DownloadToArchive = jToken.Value<bool?>("DownloadToArchive") ?? false;
            }
            else
            {
                this.Program.Read(jToken.Value<JObject>("Program") ?? new JObject());
                this.Agent.Read(jToken.Value<JObject>("Agent") ?? new JObject());
                this.Network.Read(jToken.Value<JObject>("Network") ?? new JObject());
                this.Content.Read(jToken.Value<JObject>("Content") ?? new JObject());
            }

        }

        public void Write(JToken jToken)
        {
            jToken["Version"] = 1;

            this.Program.Write(jToken["Program"] = new JObject());
            this.Agent.Write(jToken["Agent"] = new JObject());
            this.Network.Write(jToken["Network"] = new JObject());
            this.Content.Write(jToken["Content"] = new JObject());
        }

    }

}
