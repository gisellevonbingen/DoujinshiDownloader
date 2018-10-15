using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class SiteHitomi : Site
    {
        public SiteHitomi()
        {

        }

        public override string Name { get { return "Hitomi"; } }
        public override string Prefix { get { return "https://hitomi.la/galleries/"; } }
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
