using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadRequest
    {
        public GalleryValidation Validation { get; set; } = null;
        public GalleryParameterValues GalleryParameterValues { get; } = new GalleryParameterValues();
        public string FileName { get; set; } = null;

        public DownloadRequest()
        {

        }

    }

}
