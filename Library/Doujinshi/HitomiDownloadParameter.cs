using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class HitomiDownloadParameter : DownloadAgentParameter
    {
        private bool _Removed = false;
        public bool Removed { get { return this._Removed; } set { this._Removed = value; } }

        public HitomiDownloadParameter()
        {

        }

    }

}
