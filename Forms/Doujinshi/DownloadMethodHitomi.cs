using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodHitomi : DownloadMethod
    {
        public DownloadMethodHitomi()
        {

        }

        public override Site Site { get { return Site.Hitomi; } }

        public override GalleryAgent CreateAgent()
        {
            return new HitomiAgent();
        }

        public override DownloadAgentParameter CreateDownloadParameter()
        {
            var parameter = new HitomiDownloadParameter();
            parameter.Removed = false;
            return parameter;
        }

    }

}
