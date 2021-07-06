using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class ImageViewState
    {
        public GalleryImageView View { get; set; }
        public ViewState State { get; set; }
        public string ExceptionMessage { get; set; }
        public long Length { get; set; }
        public long Position { get; set; }

        public ImageViewState()
        {
            this.View = null;
            this.State = ViewState.StandBy;
            this.ExceptionMessage = null;
        }

    }

}
