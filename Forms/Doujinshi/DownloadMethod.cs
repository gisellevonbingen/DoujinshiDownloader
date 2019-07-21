using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class DownloadMethod : IEquatable<DownloadMethod>
    {
        public DownloadMethod()
        {

        }

        public abstract Site Site { get; }

        public abstract GalleryAgent CreateAgent();

        public bool Equals(DownloadMethod other)
        {
            return this == other;
        }

    }

}
