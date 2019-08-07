using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class Site
    {
        private static readonly List<Site> _Knowns = new List<Site>();

        public static Site HitomiGallery { get; }
        public static Site HitomiReader { get; }
        public static Site E_Hentai { get; }
        public static Site ExHentai { get; }

        static Site()
        {
            var knownSites = _Knowns = new List<Site>();
            knownSites.Add(HitomiGallery = new Site("Hitomi.Gallery", "https://hitomi.la/galleries/", ".html", false));
            knownSites.Add(HitomiReader = new Site("Hitomi.Reader", "https://hitomi.la/reader/", ".html", false));
            knownSites.Add(E_Hentai = new Site("E-Hentai", "https://e-hentai.org/g/", null, true));
            knownSites.Add(ExHentai = new Site("ExHentai", "https://exhentai.org/g/", null, true));
        }

        public static Site[] Knowns { get { return _Knowns.ToArray(); } }

        public Site()
        {

        }

        public string Name { get; }
        public string Prefix { get; }
        public string Suffix { get; }
        public bool RequireKey { get; }

        public Site(string name, string prefix, string suffix, bool requireKey)
        {
            this.Name = name;
            this.Prefix = prefix;
            this.Suffix = suffix;
            this.RequireKey = requireKey;
        }

        public string ToUrl(DownloadInput input)
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

        public virtual bool IsAcceptable(DownloadInput input)
        {
            if (this.RequireKey == true)
            {
                return input.Key != null;
            }

            return true;
        }

        public string SelectDownloadInput(DownloadInput input)
        {
            var builder = new StringBuilder();
            builder.Append(input.Number);

            if (this.RequireKey == true)
            {
                builder.Append(DownloadInput.KeyDelimiter);
                builder.Append(input.Key);
            }

            return builder.ToString();
        }

    }

}
