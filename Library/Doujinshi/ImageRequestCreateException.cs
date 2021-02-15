using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    [Serializable]
    public class ImageRequestCreateException : Exception
    {
        public ImageRequestCreateException()
        {
        }

        public ImageRequestCreateException(string message) : base(message)
        {

        }

        public ImageRequestCreateException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected ImageRequestCreateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }

}
