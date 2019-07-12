using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class TaskProgressingEventArgs : EventArgs
    {
        private int _Index = 0;
        public int Index { get { return this._Index; } }

        private string _URL = null;
        public string URL { get { return this._URL; } }

        private DownloadResult _Result = DownloadResult.Exception;
        public DownloadResult Result { get { return this._Result; } }

        public TaskProgressingEventArgs(int index, string url, DownloadResult result)
        {
            this._Index = index;
            this._URL = url;
            this._Result = result;
        }

    }

}
