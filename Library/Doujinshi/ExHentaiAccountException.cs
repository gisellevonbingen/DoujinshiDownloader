using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    [Serializable]
    public class ExHentaiAccountException : Exception
    {
        public ExHentaiAccountException()
        {
        }

        public ExHentaiAccountException(string message) : base(message)
        {

        }

        public ExHentaiAccountException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected ExHentaiAccountException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

    }

}
