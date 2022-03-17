using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{

    [Serializable]
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCode Code { get; private set; }

        public HttpStatusCodeException(HttpStatusCode code)
        {
            this.Code = code;
        }

        public HttpStatusCodeException(HttpStatusCode code, string message) : base(message)
        {
            this.Code = code;
        }

        public HttpStatusCodeException(HttpStatusCode code, string message, Exception inner) : base(message, inner)
        {
            this.Code = code;
        }

        protected HttpStatusCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Code = (HttpStatusCode)info.GetInt32("Code");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Code", (int)this.Code);
        }

    }

}
