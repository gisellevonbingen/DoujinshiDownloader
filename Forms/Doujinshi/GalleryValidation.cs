using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class GalleryValidation
    {
        public Site Site { get; private set; } = null;
        public DownloadInput Input { get; private set; } = default;
        public GalleryAgent Agent { get; private set; } = null;
        public bool IsError { get; private set; } = false;
        public string ErrorMessage { get; private set; } = null;
        public GalleryInfo Info { get; private set; } = null;
        public byte[] ThumbnailData { get; private set; } = null;

        private GalleryValidation()
        {

        }

        public static GalleryValidation CreateByError(Site site, string errorMessage)
        {
            return new GalleryValidation { Site = site, IsError = true, ErrorMessage = errorMessage };
        }

        public static GalleryValidation CreateByInfo(Site site, DownloadInput input, GalleryAgent agent, GalleryInfo info, byte[] thumbnailData)
        {
            return new GalleryValidation
            {
                Site = site,
                Input = input,
                Agent = agent,
                IsError = false,
                Info = info,
                ThumbnailData = thumbnailData
            };

        }

    }

}
