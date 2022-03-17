using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class GalleryValidation
    {
        public DownloadMethod Method { get; private set; } = null;
        public GalleryAgent Agent { get; private set; } = null;
        public bool IsError { get; private set; } = false;
        public string ErrorMessage { get; private set; } = null;
        public GalleryInfo Info { get; private set; } = null;
        public byte[] ThumbnailData { get; private set; } = null;

        private GalleryValidation()
        {

        }

        public static GalleryValidation CreateByError(DownloadMethod method, string errorMessage)
        {
            return new GalleryValidation { Method = method, IsError = true, ErrorMessage = errorMessage };
        }

        public static GalleryValidation CreateByInfo(DownloadMethod method, GalleryAgent agent, GalleryInfo info, byte[] thumbnailData)
        {
            return new GalleryValidation
            {
                Method = method,
                Agent = agent,
                IsError = false,
                Info = info,
                ThumbnailData = thumbnailData
            };

        }

    }

}
