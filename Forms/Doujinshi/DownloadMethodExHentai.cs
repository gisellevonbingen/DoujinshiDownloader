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
            var account = DoujinshiDownloader.Instance.Settings.Account;

            var agent = new ExHentaiAgent();
            agent.Account = account;

            return agent;
        }

        public override GalleryDownloadParameter CreateDownloadParameter()
        {
            var parameter = new ExHentaiDownloadParameter();
            parameter.Original = false;
            return parameter;
        }

    }

}
