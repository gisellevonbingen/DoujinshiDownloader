using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class GalleryInfo
    {
        public string GalleryUrl { get; set; } = null;
        public string Title { get; set; } = null;
        public string ThumbnailUrl { get; set; } = null;
        public List<GalleryParameterType> ParameterTypes { get; } = new List<GalleryParameterType>();
        public Exception Exception { get; set; } = null;
    }

}
