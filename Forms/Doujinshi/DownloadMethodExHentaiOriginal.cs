using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodExHentaiOriginal : DownloadMethodExHentai
    {
        public DownloadMethodExHentaiOriginal()
        {

        }

        public override GalleryDownloadParameter CreateDownloadParameter()
        {
            var parameter = new ExHentaiDownloadParameter();
            parameter.Original = true;
            return parameter;
        }

    }

}
