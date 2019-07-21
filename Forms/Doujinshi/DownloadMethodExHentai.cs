using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodExHentai : DownloadMethod
    {
        public DownloadMethodExHentai()
        {

        }

        public override Site Site { get { return Site.ExHentai; } }

        public override GalleryAgent CreateAgent()
        {
            var account = DoujinshiDownloader.Instance.Config.Values.Agent.ExHentaiAccount;

            var agent = new ExHentaiAgent();
            agent.Account = account;
            agent.Original = this.Original;

            return agent;
        }

        public virtual bool Original { get; }

    }

}
