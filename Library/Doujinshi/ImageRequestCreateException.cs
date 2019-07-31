using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    [Serializable]
    public class ImageRequestCreateException : Exception
    {
        public ImageRequestCreateException() { }
        public ImageRequestCreateException(string message) : base(message) { }
        public ImageRequestCreateException(string message, Exception inner) : base(message, inner) { }
        protected ImageRequestCreateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
