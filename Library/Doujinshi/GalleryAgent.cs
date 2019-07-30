using System;
using System.Collections.Generic;
using System.Drawing;
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

        public abstract GalleryInfo GetGalleryInfo(string url);

        public abstract List<string> GetGalleryImageViewURLs(string url);

        public virtual DownloadGalleryParameter CreateGalleryParameter(string url)
        {
            return new DownloadGalleryParameter { Referer = url };
        }

        public abstract GalleryImage GetGalleryImage(string viewUrl);

        public virtual byte[] GetGalleryThumbnail(string thumbnailUrl)
        {
            var req = this.CreateRequestParameter();
            req.URL = thumbnailUrl;
            req.Method = "GET";

            using (var res = this.Explorer.Request(req))
            {
                using (var rs = res.ReadToStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        rs.CopyTo(ms);
                        var bytes = ms.ToArray();
                        return bytes;
                    }

                }

            }

        }

        public virtual RequestParameter CreateImageRequest(string imageUrl, DownloadGalleryParameter galleryParameter)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Method = "GET";
            parameter.URL = imageUrl;
            parameter.Referer = galleryParameter.Referer;

            return parameter;
        }

        public abstract GalleryImage ReloadImage(string requestUrl, string reloadUrl, DownloadGalleryParameter galleryParameter);

    }

}
