using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class SiteExHentai : Site
    {
        public SiteExHentai()
        {

        }

        public override string Name { get { return "ExHentai"; } }
        public override string Prefix { get { return "https://exhentai.org/g/"; } }
        public override string Suffix { get { return null; } }

        public override bool IsAcceptable(DownloadInput input)
        {
            return input.Key != null;
        }

        public override string SelectDownloadInput(DownloadInput input)
        {
            return $"{input.Number.ToString()}/{input.Key}";
        }

    }

}
