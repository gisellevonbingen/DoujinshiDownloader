using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadRequestEventArgs : EventArgs
    {
        private DownloadRequest _Request = null;
        public DownloadRequest Request { get { return this._Request; } }

        public DownloadRequestEventArgs(DownloadRequest request)
        {
            this._Request = request;
        }

    }

}
