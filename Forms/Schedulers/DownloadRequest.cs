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
        public DownloadInput DownloadInput { get; set; } = default;
        public DownloadMethod DownloadMethod { get; set; } = null;
        public GalleryInfo2 Info { get; set; } = null;

        public DownloadRequest()
        {

        }

        public override bool Equals(object obj)
        {
            return obj is DownloadRequest other ? this.Equals(other) : false;
        }

        public override int GetHashCode()
        {
            return this.Info.Title.GetHashCode();
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
