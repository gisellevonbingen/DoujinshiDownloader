using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodExHentai : DownloadMethod<ExHentaiAgent>
    {
        public DownloadOption<bool> OptionOriginal { get; }

        public DownloadMethodExHentai(string name) : base(name)
        {
            this.Options.Add(this.OptionOriginal = new DownloadOption<bool>("Original", false));
        }

        public override Site Site { get { return Site.ExHentai; } }

        public override ExHentaiAgent CreateAgent(DownloadInput downloadInput, WebRequestProvider webRequestProvider)
        {
            var account = DoujinshiDownloader.Instance.Config.Values.Agent.ExHentaiAccount;
            return new ExHentaiAgent(this.Site, downloadInput, webRequestProvider) { Account = account?.Clone() };
        }

        public override void ApplyOptions(ExHentaiAgent agent, Dictionary<IDownloadOption, object> options)
        {
            base.ApplyOptions(agent, options);

            agent.Original = this.OptionOriginal.GetValue(options);
        }

    }

}