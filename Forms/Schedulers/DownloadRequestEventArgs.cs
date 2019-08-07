using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadRequestEventArgs : EventArgs
    {
        public DownloadRequest Request { get; }

        public DownloadRequestEventArgs(DownloadRequest request)
        {
            this.Request = request;
        }

    }

}
