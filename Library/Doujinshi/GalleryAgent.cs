using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Web;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class GalleryAgent
    {
        public WebExplorer Explorer { get; }
        public ProxySettings Proxy { get; set; }

        public GalleryAgent()
        {
            this.Explorer = new WebExplorer();
            this.Proxy = null;
        }

        public virtual RequestParameter CreateRequestParameter()
        {
            var requeset = new RequestParameter();
            requeset.Proxy = this.Proxy;

            return requeset;
        }

        public abstract string GetGalleryTitle(string url);

        public abstract List<string> GetGalleryImageViewURLs(string url);

        public virtual DownloadGalleryParameter CreateGalleryParameter(string url)
        {
            return new DownloadGalleryParameter { Referer = url };
        }

        public abstract RequestParameter GetGalleryImageDownloadRequest(string url, DownloadGalleryParameter galleryParameter);
    }

}
