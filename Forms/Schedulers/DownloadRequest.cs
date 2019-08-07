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
        public GalleryAgent Agent { get; set; } = null;
        public string GalleryTitle { get; set; } = null;
        public byte[] GalleryThumbnail { get; set; } = null;
        public string GalleryUrl { get; set; } = null;
        public string FileName { get; set; } = null;

        public DownloadRequest()
        {

        }

        public override bool Equals(object obj)
        {
            return obj is DownloadRequest other ? this.Equals(other) : false;
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash *= 31 + this.Agent?.GetHashCode() ?? 0;
            hash *= 31 + this.GalleryTitle?.GetHashCode() ?? 0;
            hash *= 31 + this.GalleryThumbnail?.GetHashCode() ?? 0;
            hash *= 31 + this.GalleryUrl?.GetHashCode() ?? 0;
            hash *= 31 + this.FileName?.GetHashCode() ?? 0;
            return hash;
        }

        public bool Equals(DownloadRequest other)
        {
            if (other == null || this.GetType().Equals(other.GetType()) == false)
            {
                return false;
            }

            if (GalleryAgent.Equals(this.Agent, other.Agent) == false)
            {
                return false;
            }

            if (string.Equals(this.GalleryTitle, other.GalleryTitle) == false)
            {
                return false;
            }

            if (Array.Equals(this.GalleryThumbnail, other.GalleryThumbnail) == false)
            {
                return false;
            }

            if (string.Equals(this.GalleryUrl, other.GalleryUrl) == false)
            {
                return false;
            }

            if (string.Equals(this.FileName, other.FileName) == false)
            {
                return false;
            }

            return true;
        }

    }

}
