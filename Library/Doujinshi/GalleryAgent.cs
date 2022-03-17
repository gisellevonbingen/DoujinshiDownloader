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
        public Site Site { get; }
        public DownloadInput DownloadInput { get; }
        private readonly WebRequestProvider WebRequestProvider;

        public GalleryAgent(Site site, DownloadInput downloadInput, WebRequestProvider webRequestProvider)
        {
            this.Site = site;
            this.DownloadInput = downloadInput;
            this.WebRequestProvider = webRequestProvider;
        }

        public string GalleryUrl => this.Site.ToUrl(this.DownloadInput);

        public WebExplorer Explorer => this.WebRequestProvider.Explorer;

        public virtual WebRequestParameter CreateRequestParameter()
        {
            return this.WebRequestProvider.CreateRequestParameter();
        }

        public abstract GalleryInfo GetGalleryInfo();

        public abstract List<GalleryImageView> GetGalleryImageViews(GalleryParameterValues values);

        public abstract GalleryImagePath GetGalleryImagePath(GalleryImageView view, GalleryParameterValues values);

        public virtual byte[] GetGalleryThumbnail(string thumbnailUrl)
        {
            var req = this.CreateThumbnailRequest(thumbnailUrl);

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

        public virtual WebRequestParameter CreateThumbnailRequest(string thumbnailUrl)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = thumbnailUrl;
            parameter.Method = "GET";

            return parameter;
        }

        public virtual WebRequestParameter CreateImageRequest(GalleryImageView view, GalleryImagePath path, GalleryParameterValues values)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Method = "GET";
            parameter.Uri = path.ImageUrl;

            return parameter;
        }

        public abstract GalleryImagePath ReloadImagePath(GalleryImageView view, GalleryImagePath prev, GalleryParameterValues values);

        public virtual void Validate(GalleryImageView view, GalleryImagePath path, GalleryParameterValues values, byte[] bytes)
        {

        }

    }

}
