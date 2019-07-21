using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodHitomiRemoved : DownloadMethod
    {
        public DownloadMethodHitomiRemoved()
        {

        }

        public override Site Site { get { return Site.HitomiRemoved; } }

        public override GalleryAgent CreateAgent()
        {
            return new HitomiAgent() { Removed = true };
        }

    }

}
