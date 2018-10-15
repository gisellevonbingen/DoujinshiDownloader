using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class Site
    {
        private static readonly List<Site> _Knowns = new List<Site>();

        private static Site _Hitomi = null;
        public static Site Hitomi { get { return _Hitomi; } }

        private static Site _HitomiRemoved = null;
        public static Site HitomiRemoved { get { return _HitomiRemoved; } }

        private static Site _E_Hentai = null;
        public static Site E_Hentai { get { return _E_Hentai; } }

        private static Site _ExHentai = null;
        public static Site ExHentai { get { return _ExHentai; } }

        static Site()
        {
            var knownSites = _Knowns = new List<Site>();
            knownSites.Add(_Hitomi = new SiteHitomi());
            knownSites.Add(_HitomiRemoved = new SiteHitomiRemoved());
            knownSites.Add(_E_Hentai = new SiteE_Hentai());
            knownSites.Add(_ExHentai = new SiteExHentai());
        }

        public static Site[] Knowns { get { return _Knowns.ToArray(); } }

        public Site()
        {

        }

        public abstract string Name { get; }
        public abstract string Prefix { get; }
        public abstract string Suffix { get; }

        public string ToURL(DownloadInput input)
        {
            var list = new List<string>();
            list.Add(this.Prefix);
            list.Add(this.SelectDownloadInput(input));
            list.Add(this.Suffix);

            var builder = new StringBuilder();

            foreach (var str in list)
            {
                if (str != null)
                {
                    builder.Append(str);
                }

            }

            return builder.ToString();
        }

        public abstract bool IsAcceptable(DownloadInput input);

        public abstract string SelectDownloadInput(DownloadInput input);

    }

}
