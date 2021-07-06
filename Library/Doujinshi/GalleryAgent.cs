using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Net;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class GalleryAgent
    {
        public WebExplorer Explorer { get; }
        public WebProxySettings Proxy { get; set; }
        public int Timeout { get; set; } = 60 * 1000;
        public int RetryCount { get; set; } = 2;

        public GalleryAgent()
        {
            this.Explorer = new WebExplorer();
            this.Proxy = null;
            this.Timeout = 0;
            this.RetryCount = 0;
        }

        public virtual WebRequestParameter CreateRequestParameter()
        {
            var req = new WebRequestParameter { Proxy = this.Proxy };
            var timeout = this.Timeout;

            if (timeout > 0)
            {
                req.Timeout = timeout;
            }

            return req;
        }

        public abstract GalleryInfo GetGalleryInfo(Site site, DownloadInput input);

        public abstract List<GalleryImageView> GetGalleryImageViews(Site site, DownloadInput input, GalleryParameterValues values);

        public abstract GalleryImagePath GetGalleryImagePath(Site site, DownloadInput input, GalleryImageView view, GalleryParameterValues values);

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

        public virtual WebRequestParameter CreateImageRequest(Site site, DownloadInput input, GalleryImageView view, GalleryImagePath path, GalleryParameterValues values)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Method = "GET";
            parameter.Uri = path.ImageUrl;

            return parameter;
        }

        public abstract GalleryImagePath ReloadImagePath(Site site, DownloadInput input, GalleryImageView view, GalleryImagePath prev, GalleryParameterValues values);

        public virtual void Validate(Site site, DownloadInput input, GalleryImageView view, GalleryImagePath path, GalleryParameterValues values, byte[] bytes)
        {

        }

    }

}
