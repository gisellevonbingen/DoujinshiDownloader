using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class DownloadMethod : IEquatable<DownloadMethod>
    {
        public static List<DownloadMethod> Knowns { get; } = new List<DownloadMethod>();

        public static DownloadMethod Hitomi { get; } = Knowns.AddAndGet(new DownloadMethodHitomi("Hitomi"));
        public static DownloadMethod E_Hentai { get; } = Knowns.AddAndGet(new DownloadMethodE_Hentai("E-Hentai"));
        public static DownloadMethod ExHentai { get; } = Knowns.AddAndGet(new DownloadMethodExHentai("ExHentai"));

        public string Name { get; }

        public DownloadMethod(string name)
        {
            this.Name = name;
        }

        public abstract Site Site { get; }

        protected abstract GalleryAgent OnCreateAgent();

        public bool Equals(DownloadMethod other)
        {
            return this == other;
        }

        public GalleryAgent CreateAgent()
        {
            var config = DoujinshiDownloader.Instance.Config.Values;

            var agent = this.OnCreateAgent();
            agent.Timeout = config.Network.Timeout;
            agent.RetryCount = config.Network.RetryCount;
            return agent;
        }

    }

}