using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodHitomi : DownloadMethod
    {
        public DownloadMethodHitomi(string name) : base(name)
        {

        }

        public override Site Site { get { return Site.Hitomi; } }

        public override GalleryAgent CreateAgent()
        {
            return new HitomiAgent() { Removed = false };
        }

    }

}
