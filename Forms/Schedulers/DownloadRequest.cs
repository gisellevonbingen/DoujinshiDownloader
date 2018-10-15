using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class DownloadRequest : IEquatable<DownloadRequest>
    {
        private DownloadInput _DownloadInput = default(DownloadInput);
        public DownloadInput DownloadInput { get { return this._DownloadInput; } set { this._DownloadInput = value; } }

        private DownloadMethod _DownloadMethod = null;
        public DownloadMethod DownloadMethod { get { return this._DownloadMethod; } set { this._DownloadMethod = value; } }

        private string _Title = null;
        public string Title { get { return this._Title; } set { this._Title = value; } }

        public DownloadRequest()
        {

        }

        public bool Equals(DownloadRequest other)
        {
            if (other == null || this.GetType().Equals(other.GetType()) == false)
            {
                return false;
            }

            if (this.DownloadInput.Equals(other.DownloadInput) == false)
            {
                return false;
            }

            if (object.Equals(this.DownloadMethod, other.DownloadMethod) == false)
            {
                return false;
            }

            return true;
        }

    }

}
