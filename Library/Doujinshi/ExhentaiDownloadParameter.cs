using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class ExHentaiDownloadParameter : GalleryDownloadParameter
    {
        private bool _Original = false;
        public bool Original { get { return this._Original; } set { this._Original = value; } }

        public ExHentaiDownloadParameter()
        {

        }

    }

}
