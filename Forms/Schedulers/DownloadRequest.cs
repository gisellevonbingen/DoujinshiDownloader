using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadRequest
    {
        public GalleryValidation Validation { get; set; } = null;
        public string FileName { get; set; } = null;

        public DownloadRequest()
        {

        }

    }

}
