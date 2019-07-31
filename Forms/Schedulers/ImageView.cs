using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class ImageView
    {
        public ViewState State { get; set; }
        public string Url { get; set; }
        public string ExceptionMessage { get; set; }

        public ImageView()
        {
            this.State = ViewState.StandBy;
            this.Url = null;
            this.ExceptionMessage = null;
        }

    }

}
