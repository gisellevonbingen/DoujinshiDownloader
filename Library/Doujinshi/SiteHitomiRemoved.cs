using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class SiteHitomiRemoved : Site
    {
        public SiteHitomiRemoved()
        {

        }

        public override string Name { get { return "Hitomi Removed"; } }
        public override string Prefix { get { return "https://hitomi.la/reader/"; } }
        public override string Suffix { get { return ".html"; } }

        public override bool IsAcceptable(DownloadInput input)
        {
            return true;
        }

        public override string SelectDownloadInput(DownloadInput input)
        {
            return input.Number.ToString();
        }

    }

}
