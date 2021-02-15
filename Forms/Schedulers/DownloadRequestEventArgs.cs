using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
