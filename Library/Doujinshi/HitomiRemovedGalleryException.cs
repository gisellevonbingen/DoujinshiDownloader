using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    [Serializable]
    public class HitomiRemovedGalleryException : Exception
    {
        public HitomiRemovedGalleryException()
        {

        }

        public HitomiRemovedGalleryException(string message) : base(message)
        {

        }

        public HitomiRemovedGalleryException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected HitomiRemovedGalleryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

    }

}
