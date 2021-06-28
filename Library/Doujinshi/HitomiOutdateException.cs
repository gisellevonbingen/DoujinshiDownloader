using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    [Serializable]
    public class HitomiOutdateException : Exception
    {
        public HitomiOutdateException()
        {

        }

        public HitomiOutdateException(string message) : base(message)
        {

        }

        public HitomiOutdateException(string message, Exception inner) : base(message, inner)
        {

        }

        protected HitomiOutdateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }

}
