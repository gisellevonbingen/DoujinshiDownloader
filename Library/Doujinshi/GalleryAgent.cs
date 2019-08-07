using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.Commons.Web;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class GalleryAgent
    {
        public WebExplorer Explorer { get; }
        public WebProxySettings Proxy { get; set; }

        public GalleryAgent()
        {
            this.Explorer = new WebExplorer();
            this.Proxy = null;
        }

        public virtual WebRequestParameter CreateRequestParameter()
        {
            var requeset = new WebRequestParameter();
            requeset.Proxy = this.Proxy;

            return requeset;
        }

        public abstract List<Site> GetSupportSites();

        public abstract GalleryInfo GetGalleryInfo(string url);

        public abstract List<string> GetGalleryImageViewURLs(string galleryUrl, GalleryParameterValues values);

        public abstract GalleryImage GetGalleryImage(string viewUrl, GalleryParameterValues values);

        public virtual byte[] GetGalleryThumbnail(string thumbnailUrl)
        {
            var req = this.CreateRequestParameter();
            req.Uri = thumbnailUrl;
            req.Method = "GET";

            using (var res = this.Explorer.Request(req))
            {
                using (var rs = res.ReadAsStream())
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

        public virtual WebRequestParameter CreateImageRequest(string imageUrl, GalleryParameterValues values)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Method = "GET";
            parameter.Uri = imageUrl;

            return parameter;
        }

        public abstract GalleryImage ReloadImage(string requestUrl, string reloadUrl, GalleryParameterValues values);

    }

}
