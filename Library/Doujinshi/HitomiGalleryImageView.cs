using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class HitomiGalleryImageView : GalleryImageView
    {
        public JToken ImageFile { get; set; }

        public HitomiGalleryImageView()
        {

        }

    }

}
