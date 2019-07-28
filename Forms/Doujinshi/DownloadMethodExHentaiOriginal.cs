using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodExHentaiOriginal : DownloadMethodExHentai
    {
        public DownloadMethodExHentaiOriginal(string name) : base(name)
        {

        }

        public override bool Original => true;

    }

}
