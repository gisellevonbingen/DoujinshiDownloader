using Giselle.DoujinshiDownloader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class DownloadMethod : IEquatable<DownloadMethod>
    {
        private static readonly List<DownloadMethod> _Knowns = new List<DownloadMethod>();

        public static DownloadMethod Hitomi { get; } = _Knowns.AddAndGet(new DownloadMethodHitomi("Hitomi"));
        public static DownloadMethod E_Hentai { get; } = _Knowns.AddAndGet(new DownloadMethodE_Hentai("E-Hentai"));
        public static DownloadMethod ExHentai { get; } = _Knowns.AddAndGet(new DownloadMethodExHentai("ExHentai"));
        public static DownloadMethod ExHentaiOriginal { get; } = _Knowns.AddAndGet(new DownloadMethodExHentaiOriginal("ExHentai_Original"));

        public static DownloadMethod[] Knowns => _Knowns.ToArray();

        public string Name { get; }

        public DownloadMethod(string name)
        {
            this.Name = name;
        }

        public abstract Site Site { get; }

        public abstract GalleryAgent CreateAgent();

        public bool Equals(DownloadMethod other)
        {
            return this == other;
        }

    }

}
