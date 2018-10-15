using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Web
{
    [Serializable]
    public class NetworkException : Exception
    {
        public NetworkException() : base()
        {

        }

        public NetworkException(string message) : base(message)
        {

        }

        public NetworkException(string message, Exception e) : base(message, e)
        {

        }

    }

}
