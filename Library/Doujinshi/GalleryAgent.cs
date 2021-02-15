using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return new WebRequestParameter { Proxy = this.Proxy };
        }

        public abstract GalleryInfo GetGalleryInfo(Site site, DownloadInput input);

        public abstract List<GalleryImageView> GetGalleryImageViews(Site site, DownloadInput input, GalleryParameterValues values);

        public abstract GalleryImagePath GetGalleryImage(Site site, DownloadInput input, string viewUrl, GalleryParameterValues values);

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

        public virtual WebRequestParameter CreateImageRequest(Site site, DownloadInput input, string imageUrl, GalleryParameterValues values)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Method = "GET";
            parameter.Uri = imageUrl;

            return parameter;
        }

        public abstract GalleryImagePath ReloadImage(Site site, DownloadInput input, string requestUrl, string reloadUrl, GalleryParameterValues values);

    }

}
